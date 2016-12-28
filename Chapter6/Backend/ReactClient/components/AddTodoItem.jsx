import React from 'react';

class AddTodoItem extends React.Component {
    static propTypes = {
        onAddItem: React.PropTypes.func.isRequired
    };

    constructor() {
        super();
        this.state = {
            text: ""
        };
    }

    onChange(event) {
        this.setState({ text: event.currentTarget.value });
        event.stopPropagation();
        return false;
    }

    onKeyPress(event) {
        if (event.keyCode === 13) {
            this.onAddClicked(event);
            event.stopPropagation();
            return true;
        }
        return false;
    }

    onAddClicked(event) {
        this.props.onAddItem({ text: this.state.text, complete: false });
        this.setState({ text: "" });
        event.stopPropagation();
        return false;
    }

    render() {
        const onChange = (event) => { return this.onChange(event); };
        const onAddClicked = (event) => { return this.onAddClicked(event); };
        const onKeyDown = (event) => { return this.onKeyPress(event); };

        return (
            <div className="addTodoItem">
                <div className="addTodoItem-text">
                    <input type="text" value={this.state.text} placeholder="Enter New Task" onChange={onChange} onKeyDown={onKeyDown}/>
                </div>
                <div className="addTodoItem-add">
                    <button onClick={onAddClicked}>Add</button>
                </div>
            </div>
        );
    }
}

export default AddTodoItem;
