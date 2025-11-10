import { useContext } from "react";
import { GlobalStoreContext } from '../../store/store.jsx'
import TextField from '@mui/material/TextField';

const RankedItem = (props) =>{

    let { index } = props;
    const { store } = useContext(GlobalStoreContext);

    async function handleOnChange() {
        store.checkValid();
    }

    return (
        <TextField
            id={'item-' + (index+1)}
            key={props.key}
            className='top5-item-editting'
            type='text'
            onChange={handleOnChange}
            defaultValue={props.text}
        />
    );
}

export default RankedItem;
