const express = require('express');
const cloudinary = require('cloudinary').v2;
const bodyParser = require('body-parser');
const cors = require('cors');

const app = express();
app.use(bodyParser.json());
app.use(cors());

cloudinary.config({
  cloud_name: process.env.Cloudinary__CloudName,
  api_key: process.env.Cloudinary__ApiKey,
  api_secret: process.env.Cloudinary__ApiSecret
});

app.post('/sign-upload', (req, res) => {
  const timestamp = Math.round((new Date()).getTime() / 1000);
  const signature = cloudinary.utils.api_sign_request({
    timestamp: timestamp,
    upload_preset: 'ml_default'
  }, process.env.Cloudinary__ApiSecret);

  res.json({ timestamp, signature });
});

const PORT = process.env.PORT || 3000;
app.listen(PORT, () => {
  console.log(`Server is running on port ${PORT}`);
});
