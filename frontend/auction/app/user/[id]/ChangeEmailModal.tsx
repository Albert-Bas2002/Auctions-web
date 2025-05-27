import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type ChangeEmailModalProps = {
  isModalVisible: boolean;
  handleChangeEmail: (
    NewEmail: string
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const ChangeEmailModal = ({
  isModalVisible,
  handleChangeEmail,
  handleCancel,
}: ChangeEmailModalProps) => {
  const [newEmail, setNewEmail] = useState("");
  const [changeMessage, setChangeMessage] = useState("");

  const onOk = async () => {
    const result = await handleChangeEmail(newEmail);

    if (result.success) {
      setChangeMessage("Email has been successfully changed");
    } else {
      setChangeMessage(result.message || "Error changing email address");
    }

    setNewEmail("");
  };
  const onCancel = () => {
    setChangeMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Change your email"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Change"
    >
      <Form layout="vertical">
        <Form.Item label="Email">
          <Input
            type="email"
            value={newEmail}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setNewEmail(e.target.value)
            }
            placeholder="you@example.com"
          />
        </Form.Item>
      </Form>
      {changeMessage && (
        <p
          style={{
            color:
              changeMessage === "Email has been successfully changed"
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

export default ChangeEmailModal;
