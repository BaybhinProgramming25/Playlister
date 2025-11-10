import { useContext } from 'react'
import { GlobalStoreContext } from '../../store/store.jsx'
import { Button, TextField} from '@mui/material';

import CloseIcon from '@mui/icons-material/HighlightOff';

import './EditToolbar.css';


const EditToolbar = () => {

    const { store } = useContext(GlobalStoreContext);
    
    function handleSave() {
        store.updateCurrentList();
    }
    
    function handlePublish() {
        store.publishCurrenList();
    }
    
    function handleClose() {
        store.closeCurrentList();
    }
    
    function handleOnChange() {
        store.checkValid();
    }
    
    let valid = true;
    if (store.valid) {
        valid = false;
    }
    
    return (
        <div id="edit-toolbar">
            <TextField
                id='list-name'
                className='edit-toolbar-textfield'
                onChange={handleOnChange}
                defaultValue={store.currentList.name}
            />
            <Button
                id='save-button'
                className='edit-toolbar-button'
                onClick={handleSave}
                variant="contained">
                Save
            </Button>
            <Button
                id='publish-button'
                className='edit-toolbar-button'
                onClick={handlePublish}
                disabled={valid}
                variant="contained">
                Publish
            </Button>
            <Button
                id='close-button'
                className='edit-toolbar-button'
                onClick={handleClose}
                variant="contained">
                <CloseIcon />
            </Button>
        </div>
    )
}

export default EditToolbar;