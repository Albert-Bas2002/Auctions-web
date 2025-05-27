"use client";
import { useParams } from "next/navigation";
import { useEffect, useState } from "react";
import { Modal, Button, Input, Form, Spin } from "antd";
import ChangeEmailModal from "./ChangeEmailModal";
import ChangePasswordModal from "./ChangePasswordModal";
import ChangeUserNameModal from "./ChangeUserNameModal";
import {
  ChangeEmail,
  ChangeUserName,
  ChangePassword,
  GetUserInfo,
  GetAuctionsCategory,
  GetAuctionPhoto,
  CreateAuction,
  ChangeContacts,
} from "../../services/api";
import { useRef } from "react";
import AuctionCard from "../../components/AuctionCard";

import { Typography, Space, Divider, Row, Col, message } from "antd";
import {
  EditOutlined,
  LockOutlined,
  PlusOutlined,
  EyeOutlined,
  TrophyOutlined,
  MailOutlined,
} from "@ant-design/icons";
import Card from "antd/es/card/Card";
import { getJwtUserId } from "../../infrastructure/jwt";
import Image from "next/image";
import dayjs from "dayjs";
import duration from "dayjs/plugin/duration";
import utc from "dayjs/plugin/utc";
import { useRouter } from "next/navigation";
import SpinItem from "@/app/components/SpinItem";
import AuctionCreateModal from "@/app/user/[id]/AuctionCreateModal";
import ChangeContactsModal from "./ChangeContactsModal";

dayjs.extend(duration);
dayjs.extend(utc);

const { Title, Text } = Typography;

interface UserProfile {
  userName: string;
  email: string;
  contacts: string;
}
interface AuctionListItem {
  auctionId: string;
  creationTime: string;
  endTime: string;
  title: string;
  reserve: number;
  currentBid: number;
}

export default function UserProfilePage() {
  const params = useParams();
  const auctionsRef = useRef<HTMLDivElement>(null);

  const id = params?.id as string;
  const [message, setMessage] = useState<string>("");
  const [userAuctions, setUserAuctions] = useState<AuctionListItem[]>([]);
  const [auctionCategory, setAuctionCategory] = useState<string | null>(null);
  const [auctionLoading, setAuctionLoading] = useState(false);
  const [isModalChangeEmailVisible, setIsModalChangeEmailVisible] =
    useState(false);
  const [isModalChangePasswordVisible, setIsModalChangePasswordVisible] =
    useState(false);
  const [isModalChangeUserNameVisible, setIsModalChangeUserNameVisible] =
    useState(false);
  const [isModalChangeContactVisible, setIsModalChangeContactsVisible] =
    useState(false);
  const [jwtUserId, setJwtUserId] = useState<string | null>(null);
  const [userProfile, setUserProfile] = useState<UserProfile | null>(null);
  const [loading, setLoading] = useState(true);
  const [imageMap, setImageMap] = useState<Record<string, string>>({});
  const [isModalCreateAuctionVisible, setIsModalCreateAuctionVisible] =
    useState(false);
  const [creatorActiveFilter, setCreatorActiveFilter] = useState<boolean>(true);

  useEffect(() => {
    const fetchData = async () => {
      setJwtUserId(getJwtUserId());
      const result = await GetUserInfo(id);
      if (
        result.success &&
        result.userName &&
        result.email &&
        result.contacts !== null &&
        result.contacts !== undefined
      ) {
        setUserProfile({
          userName: result.userName,
          email: result.email,
          contacts: result.contacts,
        });
      } else {
        if (
          result.message ===
          "The specified user ID does not exist in the system."
        ) {
          setMessage("User not found");
        } else {
          setMessage(result.message || "Server error");
        }
      }
      setLoading(false);
    };
    fetchData();
  }, []);
  useEffect(() => {
    setImageMap({});
  }, [id, auctionCategory]);
  useEffect(() => {
    async function fetchImages() {
      const newImageMap: Record<string, string> = { ...imageMap };

      for (const auction of userAuctions) {
        if (!newImageMap[auction.auctionId]) {
          const result = await GetAuctionPhoto(auction.auctionId, 0);
          newImageMap[auction.auctionId] =
            result.success && result.imageUrl
              ? result.imageUrl
              : "/default-auction.jpg";
        }
      }

      setImageMap(newImageMap);
    }

    if (userAuctions.length > 0) {
      fetchImages();
    }
  }, [userAuctions]);
  const fetchAuctionsByCategory = async (
    category: string,
    active?: boolean
  ) => {
    setAuctionLoading(true);
    setAuctionCategory(category);
    const result = await GetAuctionsCategory(category, active);
    if (result.success && result.auctionListItems) {
      setUserAuctions(result.auctionListItems);
    } else {
      setMessage(result.message ?? "Error loading auctions");
    }
    setAuctionLoading(false);
  };

  const showPasswordModal = () => setIsModalChangePasswordVisible(true);
  const showUserNameModal = () => setIsModalChangeUserNameVisible(true);
  const showEmailModal = () => setIsModalChangeEmailVisible(true);
  const showAuctionCreateModal = () => setIsModalCreateAuctionVisible(true);
  const showContactsModal = () => setIsModalChangeContactsVisible(true);

  const handleCancel = () => {
    setIsModalChangePasswordVisible(false);
    setIsModalChangeUserNameVisible(false);
    setIsModalChangeEmailVisible(false);
    setIsModalCreateAuctionVisible(false);
    setIsModalChangeContactsVisible(false);
    window.location.reload();
  };

  const handleChangeEmail = async (newEmail: string) => {
    const result = await ChangeEmail(newEmail);
    return result;
  };

  const handleChangePassword = async (
    newPassword: string,
    previousPassword: string
  ) => {
    const result = await ChangePassword(newPassword, previousPassword);
    return result;
  };

  const handleChangeUserName = async (newUserName: string) => {
    const result = await ChangeUserName(newUserName);
    return result;
  };
  const handleChangeContacts = async (newContacts: string) => {
    const result = await ChangeContacts(newContacts);
    return result;
  };

  const handleActionClick = (category: string) => {
    if (category === "Creator") {
      fetchAuctionsByCategory("Creator", creatorActiveFilter);
    } else {
      fetchAuctionsByCategory(category);
    }
    setTimeout(() => {
      auctionsRef.current?.scrollIntoView({ behavior: "smooth" });
    }, 200);
  };
  if (loading) {
    return <SpinItem />;
  }
  const handleCreateAuction = async (
    title: string,
    description: string,
    auctionDurationInDays: number,
    reserve: number
  ): Promise<{ success: boolean; message?: string }> => {
    try {
      return await CreateAuction(
        title,
        description,
        auctionDurationInDays,
        reserve
      );
    } catch (error) {
      return { success: false, message: "Registration error" };
    }
  };
  return (
    <div style={{ maxWidth: 1200, margin: "40px auto", padding: "0 16px" }}>
      <Row gutter={24} justify="center">
        <Col xs={24} md={jwtUserId === id ? 12 : 24}>
          {userProfile ? (
            <Card
              title={<Title level={3}>User Profile</Title>}
              style={{
                borderRadius: "12px",
                boxShadow: "0 4px 12px rgba(0, 0, 0, 0.1)",
              }}
            >
              <Space
                direction="vertical"
                size="large"
                style={{ width: "100%" }}
              >
                <div>
                  <Text type="secondary">User name</Text>
                  <div style={{ fontSize: "16px", fontWeight: 500 }}>
                    {userProfile.userName}
                  </div>
                </div>

                {userProfile.contacts && userProfile.contacts !== "" && (
                  <div>
                    <Text type="secondary">Contacts</Text>
                    <div style={{ fontSize: "16px", fontWeight: 500 }}>
                      {userProfile.contacts}
                    </div>
                  </div>
                )}

                {jwtUserId === id && (
                  <div>
                    <Text type="secondary">Email</Text>
                    <div style={{ fontSize: "16px", fontWeight: 500 }}>
                      {userProfile.email}
                    </div>
                  </div>
                )}

                <Divider />
              </Space>
            </Card>
          ) : (
            <Title
              level={2}
              style={{ color: "red", textAlign: "center", marginTop: 50 }}
            >
              {message || "Profile not found"}
            </Title>
          )}
        </Col>

        {jwtUserId === id && userProfile && (
          <Col xs={24} md={12}>
            <Card>
              <Space
                direction="vertical"
                size="middle"
                style={{ width: "100%" }}
              >
                <Button
                  icon={<PlusOutlined />}
                  type="primary"
                  block
                  onClick={showAuctionCreateModal}
                >
                  Create an auction
                </Button>
                <Button
                  icon={<EditOutlined />}
                  type="primary"
                  block
                  onClick={showUserNameModal}
                >
                  Change the name
                </Button>
                <Button
                  icon={<MailOutlined />}
                  type="primary"
                  block
                  onClick={showEmailModal}
                >
                  Change email
                </Button>
                <Button
                  icon={<EditOutlined />}
                  type="primary"
                  block
                  onClick={showContactsModal}
                >
                  Change contacts
                </Button>
                <Button
                  icon={<LockOutlined />}
                  type="primary"
                  block
                  onClick={showPasswordModal}
                >
                  Change password
                </Button>
                <Button
                  icon={<EyeOutlined />}
                  block
                  onClick={() => handleActionClick("Creator")}
                >
                  Where's the creator
                </Button>
                <Button
                  icon={<EyeOutlined />}
                  block
                  onClick={() => handleActionClick("Bidder")}
                >
                  Where's the bidder
                </Button>
                <Button
                  icon={<TrophyOutlined />}
                  block
                  onClick={() => handleActionClick("Winner")}
                >
                  Where's the winner
                </Button>
              </Space>
            </Card>
          </Col>
        )}
      </Row>
      {auctionCategory === "Creator" && (
        <div
          style={{
            margin: "24px 0",
            padding: "16px",
            borderRadius: "12px",
            backgroundColor: "#f9f9f9",
            textAlign: "center",
            boxShadow: "0 2px 8px rgba(0, 0, 0, 0.05)",
          }}
        >
          <Text strong style={{ marginRight: 8 }}>
            Showing:
          </Text>
          <Button
            type={creatorActiveFilter ? "primary" : "default"}
            onClick={() => {
              setCreatorActiveFilter(true);
              fetchAuctionsByCategory("Creator", true);
            }}
          >
            Active
          </Button>
          <Button
            type={!creatorActiveFilter ? "primary" : "default"}
            onClick={() => {
              setCreatorActiveFilter(false);
              fetchAuctionsByCategory("Creator", false);
            }}
            style={{ marginLeft: 8 }}
          >
            Completed
          </Button>
        </div>
      )}

      <ChangeEmailModal
        isModalVisible={isModalChangeEmailVisible}
        handleChangeEmail={handleChangeEmail}
        handleCancel={handleCancel}
      />
      <ChangePasswordModal
        isModalVisible={isModalChangePasswordVisible}
        handleChangePassword={handleChangePassword}
        handleCancel={handleCancel}
      />
      <ChangeUserNameModal
        isModalVisible={isModalChangeUserNameVisible}
        handleChangeUserName={handleChangeUserName}
        handleCancel={handleCancel}
      />
      <ChangeContactsModal
        isModalVisible={isModalChangeContactVisible}
        handleChangeContacts={handleChangeContacts}
        handleCancel={handleCancel}
      />
      <AuctionCreateModal
        isModalVisible={isModalCreateAuctionVisible}
        handleAuctionCreate={handleCreateAuction}
        handleCancel={handleCancel}
      />
      {auctionCategory && (
        <div ref={auctionsRef} style={{ marginTop: 40 }}>
          <Title level={3} style={{ textAlign: "center" }}>
            Auctions: {auctionCategory}
          </Title>
          {auctionLoading ? (
            <div
              style={{
                display: "flex",
                justifyContent: "center",
                marginTop: 40,
              }}
            >
              <Spin size="large" />
            </div>
          ) : userAuctions.length === 0 ? (
            <div style={{ textAlign: "center", marginTop: 40 }}>
              <Text type="secondary" style={{ fontSize: 18 }}>
                There are no auctions
              </Text>
            </div>
          ) : (
            <Row gutter={[24, 24]}>
              {userAuctions.map((auction) => (
                <AuctionCard
                  key={auction.auctionId}
                  auction={auction}
                  imageUrl={
                    imageMap[auction.auctionId] || "/default-auction.jpg"
                  }
                />
              ))}
            </Row>
          )}
        </div>
      )}
    </div>
  );
}
