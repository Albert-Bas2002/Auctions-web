"use client";
import React, { useEffect, useRef, useState } from "react";
import { Button, Space } from "antd";
import {
  EditOutlined,
  LockOutlined,
  PlusOutlined,
  EyeOutlined,
  MailOutlined,
  DeleteOutlined,
  DollarOutlined,
  PhoneOutlined,
} from "@ant-design/icons";
import Card from "antd/es/card/Card";
import PhotosAddModal from "./PhotosAddModal";
import PhotosDeleteModal from "./PhotosDeleteModal";
import { getJwtUserId } from "../infrastructure/jwt";
import CreateBidModal from "./CreateBidModal";

interface AuctionButtonsCardProps {
  userType: string;
  auctionStatus: string;
  photos: string[];
  handleCloseAuction: () => Promise<void>;
  handleCompleteAuction: () => Promise<void>;
  handleAddPhotos: (
    files: FileList | File[]
  ) => Promise<{ success: boolean; message?: string }>;
  handleDeletePhotos: (
    selectedIndex: number[]
  ) => Promise<{ success: boolean; message?: string }>;
  handleCreatebid: (
    amountBid: number
  ) => Promise<{ success: boolean; message?: string }>;
  handleDeleteBid: () => Promise<void>;
  handleContact: () => Promise<void>;
}

export default function AuctionButtonsCard({
  userType,
  auctionStatus,
  photos,
  handleCloseAuction,
  handleCompleteAuction,
  handleAddPhotos,
  handleDeletePhotos,
  handleCreatebid,
  handleDeleteBid,
  handleContact,
}: AuctionButtonsCardProps) {
  const fileInputRef = useRef<HTMLInputElement>(null);
  const [selectedFiles, setSelectedFiles] = useState<File[]>([]);
  const [isModalAddPhotosVisible, setIsModalAddPhotosVisible] = useState(false);
  const [isModalDeletePhotosVisible, setIsModalDeletePhotosVisible] =
    useState(false);
  const [isModalCreateBidVisible, setIsModalCreateBidVisible] = useState(false);

  const showModalDeletePhotosModal = () => setIsModalDeletePhotosVisible(true);
  const showModalCreateBidModal = () => setIsModalCreateBidVisible(true);

  const [isAuth, setIsAuth] = useState(false);

  const handleAddPhotoClick = () => {
    fileInputRef.current?.click();
  };
  useEffect(() => {
    setIsAuth(Boolean(getJwtUserId()));
  }, []);
  const handleFileChange = (e: React.ChangeEvent<HTMLInputElement>) => {
    const files = e.target.files;
    if (files) {
      setSelectedFiles((prev) => [...prev, ...Array.from(files)]);
      setIsModalAddPhotosVisible(true);
    }
    e.target.value = "";
  };

  const handlePhotoAddModalConfirm = async (): Promise<{
    success: boolean;
    message?: string;
  }> => {
    const result = await handleAddPhotos(selectedFiles);

    setSelectedFiles([]);
    return result;
  };

  const handlePhotoDeleteModalConfirm = async (
    selectedIndex: number[]
  ): Promise<{
    success: boolean;
    message?: string;
  }> => {
    const result = await handleDeletePhotos(selectedIndex);
    return result;
  };
  const handleConfirm = async (
    amountBid: number
  ): Promise<{
    success: boolean;
    message?: string;
  }> => {
    const result = await handleCreatebid(amountBid);
    return result;
  };
  const handleCancel = () => {
    setSelectedFiles([]);
    setIsModalAddPhotosVisible(false);
    setIsModalDeletePhotosVisible(false);
    setIsModalCreateBidVisible(false);
  };

  const handleModalAddFiles = (files: File[]) => {
    setSelectedFiles((prev) => [...prev, ...files]);
  };

  const canAddOrDeletePhotos =
    userType === "Creator" && auctionStatus === "Active";

  const canCloseAuction = userType === "Creator" && auctionStatus === "Active";

  const canConfirmAsCreator =
    userType === "Creator" &&
    (auctionStatus === "Deal Completed by Winner" ||
      auctionStatus === "Auction has a Winner");
  const canContact =
    (userType === "Creator" || userType === "Winner") &&
    (auctionStatus === "Auction Completely Finished" ||
      auctionStatus === "Deal Completed by Winner" ||
      auctionStatus === "Deal Completed by Creator" ||
      auctionStatus === "Auction has a Winner");

  const canConfirmAsWinner =
    userType === "Winner" &&
    (auctionStatus === "Deal Completed by Creator" ||
      auctionStatus === "Auction has a Winner");

  const canCreateBid = Boolean(
    isAuth && (userType === "Bidder" || userType === "Guest")
  );
  const canDeleteBid = userType === "Bidder";

  const shouldRender =
    canAddOrDeletePhotos ||
    canCloseAuction ||
    canConfirmAsCreator ||
    canConfirmAsWinner ||
    canCreateBid ||
    canContact ||
    canDeleteBid;

  if (!shouldRender) {
    return <div></div>;
  }

  return (
    <div>
      <input
        type="file"
        accept="image/*"
        multiple
        style={{ display: "none" }}
        ref={fileInputRef}
        onChange={handleFileChange}
      />

      <Card
        style={{
          margin: 10,
          maxWidth: 400,
          height: "fit-content",
          width: "fit-content",
          padding: 10,
          paddingTop: 24,
          marginTop: 24,
        }}
      >
        <Space direction="vertical" size="middle" style={{ width: "100%" }}>
          {canContact && (
            <Button
              icon={<PhoneOutlined />}
              type="primary"
              block
              onClick={handleContact}
            >
              Contact
            </Button>
          )}
          {canAddOrDeletePhotos && (
            <>
              <Button
                icon={<PlusOutlined />}
                type="primary"
                block
                onClick={handleAddPhotoClick}
              >
                Add photos
              </Button>
              <Button
                icon={<EditOutlined />}
                type="primary"
                block
                onClick={showModalDeletePhotosModal}
              >
                Delete photos
              </Button>
            </>
          )}

          {canConfirmAsCreator && (
            <Button
              icon={<LockOutlined />}
              type="primary"
              block
              onClick={handleCompleteAuction}
            >
              Complete deal (creator)
            </Button>
          )}

          {canConfirmAsWinner && (
            <Button
              icon={<LockOutlined />}
              type="primary"
              block
              onClick={handleCompleteAuction}
            >
              Complete deal (winner)
            </Button>
          )}

          {canCloseAuction && (
            <Button
              icon={<MailOutlined />}
              type="primary"
              block
              onClick={handleCloseAuction}
            >
              Close the auction
            </Button>
          )}

          {canCreateBid && (
            <Button
              icon={<DollarOutlined />}
              type="primary"
              block
              onClick={showModalCreateBidModal}
            >
              Create bid
            </Button>
          )}

          {canDeleteBid && (
            <Button
              icon={<DeleteOutlined />}
              danger
              block
              onClick={handleDeleteBid}
            >
              Delete bid
            </Button>
          )}
        </Space>
      </Card>

      <PhotosAddModal
        visible={isModalAddPhotosVisible}
        selectedFiles={selectedFiles}
        handleConfirm={handlePhotoAddModalConfirm}
        handleCancel={handleCancel}
        handleAddFiles={handleModalAddFiles}
      />
      <PhotosDeleteModal
        visible={isModalDeletePhotosVisible}
        photos={photos}
        handleConfirm={handlePhotoDeleteModalConfirm}
        handleCancel={handleCancel}
      />
      <CreateBidModal
        visible={isModalCreateBidVisible}
        handleConfirm={handleConfirm}
        handleCancel={handleCancel}
      />
    </div>
  );
}
