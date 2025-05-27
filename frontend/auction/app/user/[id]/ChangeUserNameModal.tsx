import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type ChangeUserNameModalProps = {
  isModalVisible: boolean;
  handleChangeUserName: (
    userName: string
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const ChangeUserNameModal = ({
  isModalVisible,
  handleChangeUserName,
  handleCancel,
}: ChangeUserNameModalProps) => {
  const [newUserName, setUserName] = useState("");
  const [changeMessage, setChangeMessage] = useState("");

  const onOk = async () => {
    const result = await handleChangeUserName(newUserName);

    if (result.success) {
      setChangeMessage("User Name has been successfully changed");
    } else {
      setChangeMessage(result.message || "Error changing user name");
    }

    setUserName("");
  };
  const onCancel = () => {
    setChangeMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Change your user name"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Change"
    >
      <Form layout="vertical">
        <Form.Item label="User Name">
          <Input
            value={newUserName}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setUserName(e.target.value)
            }
            placeholder="your user name"
          />
        </Form.Item>
      </Form>
      {changeMessage && (
        <p
          style={{
            color:
              changeMessage === "User Name has been successfully changed"
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

export default ChangeUserNameModal;
