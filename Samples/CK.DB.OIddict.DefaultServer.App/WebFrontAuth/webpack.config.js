const path = require('path');
const HtmlWebPackPlugin = require('html-webpack-plugin');

module.exports = {
  entry: {
    index: './index.ts',
    consent: './Authorization/index.ts'
  },
  output: {
    filename: 'bundle.[name].js',
    libraryTarget: 'var',
    library: ['Sample', '[name]'],
    libraryExport: 'default',
    path: path.resolve(__dirname, 'dist')
  },
  plugins: [
    new HtmlWebPackPlugin({
      template: './index.html',
      filename: 'index.html',
      chunks: ['index']
    }),
    new HtmlWebPackPlugin({
      template: './Authorization/consent.html',
      filename: './Authorization/Consent.html',
      chunks: ['consent']
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
