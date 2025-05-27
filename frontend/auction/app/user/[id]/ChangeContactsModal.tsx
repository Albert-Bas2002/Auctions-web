import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type ChangeContactsModalProps = {
  isModalVisible: boolean;
  handleChangeContacts: (
    NewEmail: string
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const ChangeContactsModal = ({
  isModalVisible,
  handleChangeContacts,
  handleCancel,
}: ChangeContactsModalProps) => {
  const [newContacts, setNewContacts] = useState("");
  const [changeMessage, setChangeMessage] = useState("");

  const onOk = async () => {
    const result = await handleChangeContacts(newContacts);

    if (result.success) {
      setChangeMessage("Contacts has been successfully changed");
    } else {
      setChangeMessage(result.message || "Error changing contacts");
    }

    setNewContacts("");
  };
  const onCancel = () => {
    setChangeMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Change your contacts"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Change"
    >
      <Form layout="vertical">
        <Form.Item label="Contacts">
          <Input
            value={newContacts}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setNewContacts(e.target.value)
            }
            placeholder="your contacts"
          />
        </Form.Item>
      </Form>
      {changeMessage && (
        <p
          style={{
            color:
              changeMessage === "Contacts has been successfully changed"
                ? "green"
                : "red",
          }}
        >
          {changeMessage}
        </p>
      )}
    </Modal>
  );
};

export default ChangeContactsModal;
