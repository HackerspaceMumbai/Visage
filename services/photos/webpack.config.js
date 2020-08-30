module.exports = {
  entry: './src/RegisterAttendeeFaceMain.jsx',
  output: {
    filename: './built/bundle.js'
  },
  module: {
    loaders: [{
      test: /.jsx?$/,
      loader: 'babel-loader',
      exclude: /node_modules/,
      query: {
        presets: ['es2015', 'react']
      }
    }]
  }
}
