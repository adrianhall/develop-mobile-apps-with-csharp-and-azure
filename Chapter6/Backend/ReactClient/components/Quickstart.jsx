import React from 'react';
import { connect } from 'react-redux';
import * as uuid from 'uuid';

import * as todoActions from '../redux/actions/todo';
import TodoItem from './TodoItem';
import AddTodoItem from './AddTodoItem';

class Quickstart extends React.Component {
    static propTypes = {
        todo: React.PropTypes.shape({
        todoItems: React.PropTypes.array
    }).isRequired,
        dispatch: React.PropTypes.func.isRequired
    };

    render() {
        const dispatch = this.props.dispatch;
        const items = this.props.todo.todoItems;

        const onDelete = (item) => {
            dispatch(todoActions.removeTodoItem(item.id));
        };

        const onUpdate = (item) => {
            dispatch(todoActions.updateTodoItem(item));
        };

        const onAddItem = (item) => {
            item.id = uuid.v1();
            dispatch(todoActions.addTodoItem(item));
        };

        const onRefresh = (event) => {
            dispatch(todoActions.refreshTodoItems());
            event.stopPropagation();
            return false;
        }

        const refreshDisabled = (this.props.todo.network > 0);
        const refreshClasses = refreshDisabled ? "qs-toolbar-refresh disabled" : "qs-toolbar-refresh";

        let errorIndicator = "";
        if (this.props.todo.error !== "") {
            errorIndicator = (<div className="error">{this.props.todo.error}</div>);
        }

        return (
            <div className="wrapper">
                <article>
                    <header>
                        <h1>Azure Mobile Apps</h1>
                        <h2>Quick Start</h2>
                        <div className="qs-toolbar">
                            <div className="qs-toolbar-additem"><AddTodoItem onAddItem={onAddItem}/></div>
                            <div className={refreshClasses}><button disabled={refreshDisabled} onClick={onRefresh}>Refresh</button></div>
                        </div>
                    </header>
                    <ul className="todoItems">
                        {items.map((item) => (<li key={item.id}><TodoItem item={item} onUpdate={onUpdate} onDelete={onDelete}/></li>))}
                    </ul>
                </article>
                    {errorIndicator}
            </div>
        );
                    }
}

export default connect((state) => {
    return {
        todo: state.todo
    };
})(Quickstart);
