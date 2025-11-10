import { useContext } from 'react';
import { GlobalStoreContext } from  '../../store/store.jsx'
import { Box } from '@mui/system';

import Button from '@mui/material/Button';

import './DeleteModal.css';

const DeleteModal = () => {

  const { store } = useContext(GlobalStoreContext);
  let msg = "";
  let deleteStatus = false;

  if (store.listMarkedForDeletion) {
    msg = "Are you sure to delete list " + store.listMarkedForDeletion.name + "?";
    deleteStatus = true;
  }

  async function handleConfirm() {
    store.deleteMarkedList();
  }

  async function handleCancle() {
    store.unmarkListForDeletion();
  }

  return (
    <div>
      <div
        aria-labelledby="unstyled-modal-title"
        aria-describedby="unstyled-modal-description"
        open={deleteStatus}
        onClose={handleCancle}
        BackdropComponent={Backdrop}
        className="styled-modal"
      >
        <Box className="modal-box">
          <h2 id="unstyled-modal-title">Double Check</h2>
          <p id="unstyled-modal-description">{msg}</p>
          <Button onClick={handleConfirm}>Confirm</Button>
          <Button onClick={handleCancle}>Cancle</Button>
        </Box>
      </div>
    </div>
  );
}

function Backdrop(props) {
  return <div {...props} className="modal-backdrop" />;
}

export default DeleteModal;