import express from 'express';
import { v2 as cloudinary } from 'cloudinary';
import bodyParser from 'body-parser';
import cors from 'cors';

//Below imports are from the aspire samples https://github.com/dotnet/aspire-samples/tree/main/samples/AspireWithNode
import http from 'node:http';
import https from 'node:https';
import fs from 'node:fs';
import { createTerminus, HealthCheckError } from '@godaddy/terminus';


// Read configuration from environment variables
const config = {
  environment: process.env.NODE_ENV || 'development',
  httpPort: process.env['PORT'] ?? 8080,
  httpsPort: process.env['HTTPS_PORT'] ?? 8443,
  httpsRedirectPort: process.env['HTTPS_REDIRECT_PORT'] ?? (process.env['HTTPS_PORT'] ?? 8443),
  certFile: process.env['HTTPS_CERT_FILE'] ?? '',
  certKeyFile: process.env['HTTPS_CERT_KEY_FILE'] ?? '',
  cacheAddress: process.env['ConnectionStrings__cache'] ?? '',
  apiServer: process.env['services__weatherapi__https__0'] ?? process.env['services__weatherapi__http__0']
};
console.log(`Configuration loaded. Environment: ${config.environment}, HTTP Port: ${config.httpPort}, HTTPS Port: ${config.httpsPort}`);


// Setup HTTPS options
const httpsOptions = fs.existsSync(config.certFile) && fs.existsSync(config.certKeyFile)
    ? {
        cert: fs.readFileSync(config.certFile),
        key: fs.readFileSync(config.certKeyFile),
        enabled: true
    }
    : { enabled: false };

const app = express();
app.use(bodyParser.json());
app.use(cors());

cloudinary.config({
  cloud_name: process.env.Cloudinary__CloudName,
  api_key: process.env.Cloudinary__ApiKey,
  api_secret: process.env.Cloudinary__ApiSecret
});

app.get('/sign-upload', (req, res) => {
  const timestamp = Math.round((new Date()).getTime() / 1000);
  const signature = cloudinary.utils.api_sign_request({
    timestamp: timestamp,

  }, process.env.Cloudinary__ApiSecret);

  res.json({ timestamp, signature });
});

//Below is from the aspire samples https://github.com/dotnet/aspire-samples/tree/main/samples/AspireWithNode
function httpsRedirect(req, res, next) {
  if (req.secure || req.headers['x-forwarded-proto'] === 'https') {
      // Request is already HTTPS
      return next();
  }
  // Redirect to HTTPS
  const redirectTo = new URL(`https://${process.env.HOST ?? 'localhost'}:${config.httpsRedirectPort}${req.url}`);
  console.log(`Redirecting to ${redirectTo}`);
  res.redirect(redirectTo);
}
if (httpsOptions.enabled) {
  app.use(httpsRedirect);
}


// Define health check callback
async function healthCheck() {
  const errors = [];
  const apiServerHealthAddress = `${config.apiServer}/health`;
  console.log(`Fetching ${apiServerHealthAddress}`);
  try {
      var response = await fetch(apiServerHealthAddress);
      if (!response.ok) {
          console.log(`Failed fetching ${apiServerHealthAddress}. ${response.status}`);
          throw new HealthCheckError(`Fetching ${apiServerHealthAddress} failed with HTTP status: ${response.status}`);
      }
  } catch (error) {
      console.log(`Failed fetching ${apiServerHealthAddress}. ${error}`);
      throw new HealthCheckError(`Fetching ${apiServerHealthAddress} failed with HTTP status: ${error}`);
  }
}

// Start a server
function startServer(server, port) {
  if (server) {
      const serverType = server instanceof https.Server ? 'HTTPS' : 'HTTP';

      // Create the health check endpoint
      createTerminus(server, {
          signal: 'SIGINT',
          healthChecks: {
              '/health': healthCheck,
              '/alive': () => { }
          },
          onSignal: async () => {
              console.log('server is starting cleanup');
              console.log('closing Redis connection');
              await cache.disconnect();
          },
          onShutdown: () => console.log('cleanup finished, server is shutting down')
      });

      // Start the server
      server.listen(port, () => {
          console.log(`${serverType} listening on ${JSON.stringify(server.address())}`);
      });
  }
}

const httpServer = http.createServer(app);
const httpsServer = httpsOptions.enabled ? https.createServer(httpsOptions, app) : null;

startServer(httpServer, config.httpPort);
startServer(httpsServer, config.httpsPort);
