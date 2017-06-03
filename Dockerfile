FROM node:7.2

COPY . /src  
RUN cd /src/visage-isomorphic; npm install

EXPOSE 4000

CMD ["nodejs", "/src/visage-isomorphic/server.js"] 
