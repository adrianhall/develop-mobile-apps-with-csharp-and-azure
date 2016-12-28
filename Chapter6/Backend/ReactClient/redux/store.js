import { createStore, combineReducers, applyMiddleware } from 'redux';
import createLogger from 'redux-logger';
import thunkMiddleware from 'redux-thunk';
import promiseMiddleware from 'redux-promise';

import * as reducers from './reducers';
import * as todoActions from './actions/todo';

const appReducers = combineReducers({ ...reducers });

const reduxStore = applyMiddleware(
    thunkMiddleware,
    promiseMiddleware,
    createLogger()
)(createStore);

export const store = reduxStore(appReducers);

// Dispatch a refresh action
store.dispatch(todoActions.refreshTodoItems());
