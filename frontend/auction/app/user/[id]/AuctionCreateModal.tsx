import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type AuctionCreateModalProps = {
  isModalVisible: boolean;
  handleAuctionCreate: (
    title: string,
    description: string,
    auctionDurationInDays: number,
    reserve: number
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const AuctionCreateModal = ({
  isModalVisible,
  handleAuctionCreate,
  handleCancel,
}: AuctionCreateModalProps) => {
  const [title, setTitle] = useState("");
  const [description, setDescription] = useState("");
  const [reserve, setReserve] = useState<number>(0);
  const [auctionDurationInDays, setAuctionDurationInDays] = useState<number>(3);
  const [auctionCreateMessage, setAuctionCreateMessage] = useState("");

  const onOk = async () => {
    const result = await handleAuctionCreate(
      title,
      description,
      auctionDurationInDays,
      reserve
    );

    if (result.success) {
      setAuctionCreateMessage("A successful auction has been set up");
    } else {
      setAuctionCreateMessage(result.message || "Auction creation error");
    }

    setTitle("");
    setDescription("");
    setReserve(0);
    setAuctionDurationInDays(3);
  };
  const onCancel = () => {
    setAuctionCreateMessage("");
    handleCancel();
  };
  return (
    <Modal
      title="Auction Create"
      open={isModalVisible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Create"
    >
      <Form layout="vertical">
        <Form.Item label="Title">
          <Input
            value={title}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setTitle(e.target.value)
            }
            placeholder="title"
          />
        </Form.Item>
        <Form.Item label="Description">
          <Input
            value={description}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setDescription(e.target.value)
            }
            placeholder="description"
          />
        </Form.Item>
        <Form.Item label="Reserve">
          <Input
            type="number"
            value={reserve}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setReserve(Number(e.target.value))
            }
            placeholder="reserve"
          />
        </Form.Item>
        <Form.Item label="Durations in days">
          <Input
            type="number"
            value={auctionDurationInDays}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setAuctionDurationInDays(Number(e.target.value))
            }
            placeholder="durations"
          />
        </Form.Item>
      </Form>
      {auctionCreateMessage && (
        <p
          style={{
            color:
              auctionCreateMessage === "A successful auction has been set up"
                ? "green"
                : "red",
          }}
        >
          {auctionCreateMessage}
        </p>
      )}
    </Modal>
  );
};

export default AuctionCreateModal;
