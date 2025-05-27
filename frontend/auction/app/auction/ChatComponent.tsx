import React, { useState, useEffect, useRef } from "react";
import { List, Input, Button } from "antd";
import { SendOutlined } from "@ant-design/icons";

const { TextArea } = Input;

interface Message {
  userCategory: string;
  message: string;
  timestamp: number;
}

interface ChatComponentProps {
  messages: Message[];
  onSend: (msg: string) => void;
}

export default function ChatComponent({
  messages,
  onSend,
}: ChatComponentProps) {
  const [message, setMessage] = useState("");

  const handleSend = () => {
    if (message.trim()) {
      onSend(message);
      setMessage("");
    }
  };

  return (
    <div
      style={{
        display: "flex",
        flexDirection: "column",
        height: "350px", // поменьше по высоте
        width: "600px", // пошире
        border: "1px solid #ccc",
        borderRadius: 4,
      }}
    >
      <div
        style={{
          flex: 1,
          overflowY: "auto",
          padding: 16,
          backgroundColor: "#fafafa",
        }}
      >
        <List
          dataSource={messages}
          renderItem={(item) => (
            <List.Item>
              <List.Item.Meta
                title={item.userCategory}
                description={item.message}
              />
              <div style={{ fontSize: 12, color: "#999" }}>
                {new Date(item.timestamp).toLocaleTimeString()}
              </div>
            </List.Item>
          )}
        />
      </div>

      <div style={{ padding: 16, borderTop: "1px solid #f0f0f0" }}>
        <TextArea
          rows={2}
          value={message}
          onChange={(e: React.ChangeEvent<HTMLTextAreaElement>) =>
            setMessage(e.target.value)
          }
          onPressEnter={(e: React.KeyboardEvent<HTMLTextAreaElement>) => {
            if (!e.shiftKey) {
              e.preventDefault();
              handleSend();
            }
          }}
        />
        <div style={{ textAlign: "right", marginTop: 8 }}>
          <Button type="primary" icon={<SendOutlined />} onClick={handleSend}>
            Отправить
          </Button>
        </div>
      </div>
    </div>
  );
}
