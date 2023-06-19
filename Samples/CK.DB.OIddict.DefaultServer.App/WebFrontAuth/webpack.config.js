const path = require('path');
const HtmlWebPackPlugin = require('html-webpack-plugin');

module.exports = {
  entry: './index.ts',
  output: {
    filename: 'bundle.js',
    libraryTarget: 'var',
    library: 'Sample',
    libraryExport: 'default',
    path: path.resolve(__dirname, 'dist')
  },
  plugins: [
    new HtmlWebPackPlugin({
      template: './index.html',
      filename: './index.html'
    }),
    new HtmlWebPackPlugin({
      template: './style.css',
      filename: './style.css'
    })
  ],
  mode: 'development',
  devtool: 'inline-source-map',
  devServer: {
    contentBase: '.',
    watchContentBase: true
  },
  module: {
    rules: [
      {
        test: /\.tsx?$/,
        use: 'ts-loader',
        exclude: /node_modules/
      }
    ]
  },
  resolve: {
    extensions: [ '.tsx', '.ts', '.js' ]
  }
};