'use strict';
/* global __dirname */

var path = require('path'),
    webpack = require('webpack');

var configuration = {
    devtool: 'source-map',
    entry: {
        app: [
            path.join(__dirname, 'ReactClient/app.jsx')
        ],
        vendor: [
            'azure-mobile-apps-client',
            'react',
            'react-dom',
            'react-redux',
            'redux',
            'redux-logger',
            'redux-promise',
            'redux-thunk',
            'uuid'
        ]
    },
    module: {
        loaders: [
            { test: /\.jsx?$/, loader: 'babel', exclude: /node_modules/ },
            { test: /\.json$/, loader: 'json' }
        ]
    },
    output: {
        path: path.join(__dirname, 'Content/spa/react'),
        publicPath: '/',
        filename: 'app.js'
    },
    plugins: [
        new webpack.optimize.CommonsChunkPlugin('vendor', 'vendor.bundle.js'),
        new webpack.optimize.DedupePlugin()
    ],
    resolve: {
        modulesDirectories: ['node_modules'],
        extensions: ['', '.js', '.jsx']
    },
    target: 'web'
};

module.exports = configuration;
