FROM node:7.2-slim

#RUN mkdir -p /opt/app
RUN useradd --user-group --create-home --shell /bin/false app &&   mkdir -p /opt/app
#RUN groupadd -r app && useradd -r -g app -d /opt/app -s /sbin/nologin -c "Docker image user" app && mkdir -p /opt/app

# set our node environment, either development or production
# defaults to production, compose overrides this to development on build and run
ARG NODE_ENV=production
ENV NODE_ENV $NODE_ENV

# default to port 8080 for node, and 5858 or 9229 for debug
ARG PORT=8080
ENV PORT $PORT
EXPOSE $PORT 5858 9229

# check every 30s to ensure this service returns HTTP 200
HEALTHCHECK CMD curl -fs http://localhost:$PORT/healthz || exit 1

# install dependencies first, in a different location for easier app bind mounting for local development
WORKDIR /opt
COPY package.json /opt
RUN npm install && npm cache clean --force
ENV PATH /opt/node_modules/.bin:$PATH

RUN chown -R app:app /opt/app && chgrp -R app /opt/app
USER app

# copy in our source code last, as it changes the most
WORKDIR /opt/app
COPY . /opt/app

CMD ["node", "server.js"] 
