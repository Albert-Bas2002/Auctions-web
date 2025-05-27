import React, { useRef, useState } from "react";
import { Modal, Space } from "antd";
import Image from "next/image";
interface PhotosModalProps {
  visible: boolean;
  selectedFiles: File[];
  handleConfirm: () => Promise<{ success: boolean; message?: string }>;
  handleCancel: () => void;
  handleAddFiles: (files: File[]) => void;
}

export default function PhotosAddModal({
  visible,
  selectedFiles,
  handleConfirm,
  handleCancel,
  handleAddFiles,
}: PhotosModalProps) {
  const modalFileInputRef = useRef<HTMLInputElement>(null);
  const [message, setMessage] = useState<string>("");

  const handleModalAddPhotoClick = () => {
    modalFileInputRef.current?.click();
  };

  const handleModalFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files) {
      handleAddFiles(Array.from(files));
    }
    e.target.value = "";
  };
  const onOK = async () => {
    var result = await handleConfirm();
    if (result.success) {
      setMessage("Photo successfully added");
    } else {
      setMessage(result.message || "Error adding photo");
    }
  };
  const onCancel = async () => {
    handleCancel();
    setMessage("");
    window.location.reload();
  };

  return (
    <>
      <input
        type="file"
        accept="image/*"
        multiple
        style={{ display: "none" }}
        ref={modalFileInputRef}
        onChange={handleModalFileChange}
      />
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
          <div
            onClick={handleModalAddPhotoClick}
            style={{
              width: 100,
              height: 100,
              border: "2px dashed #1890ff",
              display: "flex",
              alignItems: "center",
              justifyContent: "center",
              cursor: "pointer",
              borderRadius: 8,
              fontSize: 40,
              color: "#1890ff",
              userSelect: "none",
            }}
          >
            +
          </div>

          {selectedFiles.map((file, index) => {
            const url = URL.createObjectURL(file);
            return (
              <Image
                key={index}
                src={url}
                alt={`preview-${index}`}
                width={100}
                height={100}
                style={{ objectFit: "cover", borderRadius: 8 }}
                onLoad={() => URL.revokeObjectURL(url)}
              />
            );
          })}
        </Space>
        {message && (
          <p
            style={{
              color: message === "Photo successfully added" ? "green" : "red",
            }}
          >
            {message}
          </p>
        )}
      </Modal>
    </>
  );
}
