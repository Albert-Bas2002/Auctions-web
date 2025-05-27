import React, { useState } from "react";
import { Modal, Space } from "antd";
import Image from "next/image";

interface PhotosModalProps {
  visible: boolean;
  photos: string[];
  handleConfirm: (
    selectedIndexes: number[]
  ) => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
}

export default function PhotosDeleteModal({
  visible,
  photos,
  handleConfirm,
  handleCancel,
}: PhotosModalProps) {
  const [message, setMessage] = useState<string>("");
  const [selectedIndexes, setSelectedIndexes] = useState<number[]>([]);

  const onOK = async () => {
    const result = await handleConfirm(selectedIndexes);
    if (result.success) {
      setMessage("The photos have been successfully deleted");
    } else {
      setMessage(result.message || "Photo deletion error");
    }
  };

  const onCancel = () => {
    handleCancel();
    setMessage("");
    setSelectedIndexes([]);
    window.location.reload();
  };

  const handleSelect = (index: number) => {
    setSelectedIndexes((prev) =>
      prev.includes(index) ? prev.filter((i) => i !== index) : [...prev, index]
    );
  };

  return (
    <Modal
      title="Confirm added photos"
      open={visible}
      onOk={onOK}
      onCancel={onCancel}
      okText="Confirm"
      cancelText="Cancel"
      width={600}
    >
      <Space wrap size="middle">
        {photos.length === 0 && <p>No photos available</p>}
        {photos.map((photo, index) => (
          <div
            key={index}
            onClick={() => handleSelect(index)}
            style={{
              border: selectedIndexes.includes(index)
                ? "3px solid #1890ff"
                : "1px solid #ccc",
              borderRadius: 8,
              cursor: "pointer",
              padding: 4,
            }}
          >
            <Image
              src={photo}
              alt={`preview-${index}`}
              width={100}
              height={100}
              style={{ objectFit: "cover", borderRadius: 4 }}
            />
          </div>
        ))}
      </Space>

      {message && (
        <p
          style={{
            marginTop: 16,
            color:
              message === "The photos have been successfully deleted"
                ? "green"
                : "red",
          }}
        >
          {message}
        </p>
      )}
    </Modal>
  );
}
