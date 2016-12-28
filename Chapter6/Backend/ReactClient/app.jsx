import React from 'react';
import ReactDOM from 'react-dom';
import { Provider } from 'react-redux';
import { store } from './redux/store';
import Quickstart from './components/Quickstart';

ReactDOM.render(
    <Provider store={store}>
        <Quickstart/>
    </Provider>
, document.getElementById('app')
);
