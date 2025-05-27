import React, { useState } from "react";
import { Modal, Button, Input, Form } from "antd";

type CreateBidModalProps = {
  visible: boolean;
  handleConfirm: (
    amountBid: number
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
};

const CreateBidModal = ({
  visible,
  handleConfirm,
  handleCancel,
}: CreateBidModalProps) => {
  const [newBid, setNewBid] = useState<number>(0);
  const [message, setMessage] = useState<string>("");

  const onOk = async () => {
    const result = await handleConfirm(newBid);

    if (result.success) {
      setMessage("Bid successfully posted");
    } else {
      setMessage(result.message || "Bid creation error");
    }

    setNewBid(0);
  };
  const onCancel = () => {
    setMessage("");
    setNewBid(0);
    handleCancel();
  };
  return (
    <Modal
      title="Create bid"
      open={visible}
      onCancel={onCancel}
      onOk={onOk}
      okText="Bid"
    >
      <Form layout="vertical">
        <Form.Item label="Bid">
          <Input
            type="number"
            value={newBid}
            onChange={(e: React.ChangeEvent<HTMLInputElement>) =>
              setNewBid(Number(e.target.value))
            }
            placeholder="your bid"
          />
        </Form.Item>
      </Form>
      {message && (
        <p
          style={{
            color: message === "Bid successfully posted" ? "green" : "red",
          }}
        >
          {message}
        </p>
      )}
    </Modal>
  );
};

export default CreateBidModal;
