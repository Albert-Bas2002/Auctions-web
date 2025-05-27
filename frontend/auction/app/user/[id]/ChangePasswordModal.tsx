import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type ChangePasswordModalProps = {
  isModalVisible: boolean;
  handleChangePassword: (
    newPassword: string,
    previousPassword: string
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const ChangePasswordModal = ({
  isModalVisible,
  handleChangePassword,
  handleCancel,
}: ChangePasswordModalProps) => {
  const [newPassword, setNewPassword] = useState("");
  const [previousPassword, setPreviousPassword] = useState("");

  const [changeMessage, setChangeMessage] = useState("");

  const onOk = async () => {
    const result = await handleChangePassword(newPassword, previousPassword);

    if (result.success) {
      setChangeMessage("Password successfully changed");
    } else {
      setChangeMessage(result.message || "Password change error");
    }

    setNewPassword("");
    setPreviousPassword("");
  };
  const onCancel = () => {
    setChangeMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Change your password"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Change"
    >
      <Form layout="vertical">
        <Form.Item label="New Password">
          <Input
            type="password"
            value={newPassword}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setNewPassword(e.target.value)
            }
            placeholder="your new password"
          />
        </Form.Item>

        <Form.Item label="Previous Password">
          <Input
            type="password"
            value={previousPassword}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setPreviousPassword(e.target.value)
            }
            placeholder="your previous password"
          />
        </Form.Item>
      </Form>
      {changeMessage && (
        <p
          style={{
            color:
              changeMessage === "Password successfully changed"
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

export default ChangePasswordModal;
