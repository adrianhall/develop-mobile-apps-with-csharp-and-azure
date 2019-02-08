import React from 'react';

class TodoItem extends React.Component {
    static PropTypes = {
        item: React.PropTypes.shape({
        id: React.PropTypes.string.isRequired,
        text: React.PropTypes.string.isRequired,
        complete: React.PropTypes.bool.isRequired
    }).isRequired,
        onDelete: React.PropTypes.func,
        onUpdate: React.PropTypes.func
    };

onTextUpdate(event) {
        let newItem = Object.assign({}, this.props.item, {
            text: event.currentTarget.value
        });
        this.props.onUpdate(newItem);
        event.stopPropagation();
        return false;
    }

    onCompleteUpdate(event) {
        let newItem = Object.assign({}, this.props.item, {
            complete: event.currentTarget.checked
        });
        this.props.onUpdate(newItem);
        event.stopPropagation();
        return false;
    }

    onDelete(event) {
        if (confirm('Are you sure you want to delete this record?')) {
            this.props.onDelete(this.props.item);
        }
        event.stopPropagation();
        return false;
    }

    render() {
        const onTextUpdate = (event) => { return this.onTextUpdate(event); };
        const onCompleteUpdate = (event) => { return this.onCompleteUpdate(event); };
        const onDelete = (event) => { return this.onDelete(event); };
        const item = this.props.item;

        return (
            <div className="todoItem">
                <div className="todoItem-id">{item.id}</div>
                <div className="todoItem-text"><input type="text" value={item.text} onChange={onTextUpdate}/></div>
                <div className="todoItem-complete"><input type="checkbox" checked={item.complete} onChange={onCompleteUpdate}/></div>
                <div className="todoItem-delete"><button onClick={onDelete}>Delete</button></div>
            </div>
        );
    }
}

export default TodoItem;
