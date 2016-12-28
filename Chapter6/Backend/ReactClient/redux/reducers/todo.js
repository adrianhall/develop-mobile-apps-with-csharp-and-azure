import constants from '../constants';

const initialState = {
    todoItems: [],
    network: 0,
    error: ""
};

export default function todoReducers(state = initialState, action) {
    switch (action.type) {
        case constants.todo.addItem:
            return Object.assign({}, state, {
                todoItems: state.todoItems.concat(action.item)
            });

        case constants.todo.removeItem:
            return Object.assign({}, state, {
                todoItems: state.todoItems.filter(item => item.id !== action.id)
            });

        case constants.todo.updateItem:
            return Object.assign({}, state, {
                todoItems: state.todoItems.map(item => { return (item.id === action.item.id) ? action.item : item; })
            });

        case constants.todo.replaceItems:
            return Object.assign({}, state, {
                todoItems: action.items
            });

        case constants.todo.network:
            return Object.assign({}, state, {
                network: state.network + action.counter
            });

        case constants.todo.error:
            return Object.assign({}, state, {
                error: action.error
            });

        default:
            return state;
    }
};
