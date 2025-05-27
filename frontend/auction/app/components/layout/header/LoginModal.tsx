import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type LoginModalProps = {
  isModalVisible: boolean;
  handleLogin: (
    email: string,
    password: string
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const LoginModal = ({
  isModalVisible,
  handleLogin,
  handleCancel,
}: LoginModalProps) => {
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [loginMessage, setLoginMessage] = useState<string>("");

  const onOk = async () => {
    const result = await handleLogin(email, password);

    if (result.success) {
      setLoginMessage("Successful entry");
    } else {
      setLoginMessage(result.message || "Login error");
    }

    setEmail("");
    setPassword("");
  };
  const onCancel = () => {
    setLoginMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Login"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Login"
    >
      <Form layout="vertical">
        <Form.Item label="Email">
          <Input
            type="email"
            value={email}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setEmail(e.target.value)
            }
            placeholder="you@example.com"
          />
        </Form.Item>
        <Form.Item label="Password">
          <Input.Password
            value={password}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setPassword(e.target.value)
            }
            placeholder="Enter your password"
          />
        </Form.Item>
      </Form>
      {loginMessage && (
        <p
          style={{
            color: loginMessage === "Successful entry" ? "green" : "red",
          }}
        >
          {loginMessage}
        </p>
      )}
    </Modal>
  );
};

export default LoginModal;
