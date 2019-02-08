import * as WindowsAzure from 'azure-mobile-apps-client';
import constants from '../constants';

const ZUMOAPPURL = location.origin;

const zumoClient = new WindowsAzure.MobileServiceClient(ZUMOAPPURL);
const todoTable = zumoClient.getTable('todoitem');

function networkProcess(counter) {
    return {
        type: constants.todo.network,
        counter: counter
    };
}

function zumoError(error) {
    return {
        type: constants.todo.error,
        error: error
    };
}

export function addTodoItem(item) {
    return (dispatch) => {
        dispatch({
            type: constants.todo.addItem,
            item: item
        });

        const success = (item) => {
            dispatch({
                type: constants.todo.updateItem,
                item: item
            });
        };
        const failure = (error) => { dispatch(zumoError(error)); };

        dispatch(networkProcess(1));
        todoTable.insert(item).done(success, failure);
        dispatch(networkProcess(-1));
    };
}

export function removeTodoItem(id) {
    return (dispatch) => {
        dispatch({
            type: constants.todo.removeItem,
            id: id
        });

        const success = () => {};
        const failure = (error) => { dispatch(zumoError(error)); };

        dispatch(networkProcess(1));
        todoTable.del({ id: id }).done(success, failure);
        dispatch(networkProcess(-1));
    };
}

export function updateTodoItem(item) {
    return (dispatch) => {
        dispatch({
            type: constants.todo.updateItem,
            item: item
        });

        const success = (item) => {
            dispatch({
                type: constants.todo.updateItem,
                item: item
            });
        };
        const failure = (error) => { dispatch(zumoError(error)); };

        dispatch(networkProcess(1));
        todoTable.update(item).done(success, failure);
        dispatch(networkProcess(-1));
    };

}

export function refreshTodoItems() {
    return (dispatch) => {
        const success = (data) => {
            dispatch({
                type: constants.todo.replaceItems,
                items: data
            });
        };
        const failure = (error) => { dispatch(zumoError(error)); };

        dispatch(networkProcess(1));
        todoTable.read().then(success, failure);
        dispatch(networkProcess(-1));
    }
}