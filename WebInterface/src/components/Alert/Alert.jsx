import { useContext } from "react";
import { GlobalStoreContext } from '../../store/store.jsx';

import Box from '@mui/material/Box';
import Alert from '@mui/material/Alert';
import IconButton from '@mui/material/IconButton';
import Collapse from '@mui/material/Collapse';
import CloseIcon from '@mui/icons-material/Close';

import './Alert.css';

const Alerts = () => {

    const { store } = useContext(GlobalStoreContext);
    let msg = "";
    let alertSatus = false;

    if (store.alertMessage) {
        msg = store.alertMessage;
    }
    if (store.alertSatus) {
        alertSatus = true;
    }

  return (
    <Box id='alert-box' className="alert-box">
      <Collapse in={alertSatus} id='collapse'>
        <Alert id='alert'
            className="alert"
            severity="error"
            action={
            <IconButton
              aria-label="close"
              color="inherit"
              size="small"
              onClick={() => { store.hideAlert(); }}
            >
              <CloseIcon fontSize="inherit" />
            </IconButton>
          }
        >
        {msg}
        </Alert>
      </Collapse>
    </Box>
  );
}

export default Alerts; 