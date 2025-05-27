import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type RegisterModalProps = {
  isModalVisible: boolean;
  handleRegister: (
    userName: string,
    email: string,
    password: string,
    contacts: string
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const RegisterModal = ({
  isModalVisible,
  handleRegister,
  handleCancel,
}: RegisterModalProps) => {
  const [userName, setUserName] = useState("");
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [contacts, setContacts] = useState("");

  const [registerMessage, setRegisterMessage] = useState<string>("");

  const onOk = async () => {
    const result = await handleRegister(userName, email, password, contacts);

    if (result.success) {
      setRegisterMessage("Successful registration");
    } else {
      setRegisterMessage(result.message || "Registration error");
    }
    setEmail("");
    setPassword("");
    setUserName("");
    setContacts("");
  };
  const onCancel = () => {
    setRegisterMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Register"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Register"
    >
      <Form layout="vertical">
        <Form.Item label="User Name">
          <Input
            value={userName}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setUserName(e.target.value)
            }
            placeholder="Enter your name"
          />
        </Form.Item>
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
        <Form.Item label="Contacts">
          <Input
            value={contacts}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setContacts(e.target.value)
            }
            placeholder="you contacts"
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
      {registerMessage && (
        <p
          style={{
            color:
              registerMessage === "Successful registration" ? "green" : "red",
          }}
        >
          {registerMessage}
        </p>
      )}
    </Modal>
  );
};

export default RegisterModal;
