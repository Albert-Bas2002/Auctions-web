"use client";
import dayjs from "dayjs";
import duration from "dayjs/plugin/duration";
dayjs.extend(duration);

import {
  HubConnectionBuilder,
  HubConnection,
  HubConnectionState,
  LogLevel,
} from "@microsoft/signalr";

import { useParams, useRouter } from "next/navigation";
import { useEffect, useRef, useState } from "react";
import { Typography } from "antd";
import {
  AddAuctionPhotos,
  DeleteAuctionPhotos,
  GetAuctionPage,
  GetAuctionPhoto,
  CloseAuction,
  CompleteAuctionDeal,
  GetCreatorOrWinnerId,
} from "../../services/api";

import SpinItem from "@/app/components/SpinItem";
import AuctionPageCard from "@/app/auction/AuctionPageCard";
import AuctionButtonsCard from "@/app/auction/AuctionButtonsCard";
import ChatComponent from "../ChatComponent";

const { Title } = Typography;

function getRemainingTime(endTime: string) {
  const now = dayjs();
  const end = dayjs(endTime);
  const diff = end.diff(now);

  if (diff <= 0) return "Ended";

  const duration = dayjs.duration(diff);
  if (duration.asDays() >= 1) {
    return `${Math.floor(duration.asDays())} day${
      Math.floor(duration.asDays()) > 1 ? "s" : ""
    } left`;
  }

  const hours = String(duration.hours()).padStart(2, "0");
  const minutes = String(duration.minutes()).padStart(2, "0");
  const seconds = String(duration.seconds()).padStart(2, "0");

  return `${hours}:${minutes}:${seconds}`;
}
interface Message {
  userCategory: string;
  message: string;
  timestamp: number;
}
const AuctionPage = () => {
  const connectionRef = useRef<HubConnection | null>(null);
  const [message, setMessage] = useState<string>("");
  const [auction, setAuction] = useState<any>(null);
  const [photos, setPhotos] = useState<string[]>([]);
  const [loading, setLoading] = useState<boolean>(true);
  const [remainingTime, setRemainingTime] = useState("");
  const router = useRouter();

  const [messages, setMessages] = useState<Message[]>([]);
  const params = useParams();
  const id = params?.id as string;
  useEffect(() => {
    window.scrollTo(0, 0);
  }, []);
  useEffect(() => {
    if (!auction) return;
    const interval = setInterval(() => {
      setRemainingTime(getRemainingTime(auction.endTime));
    }, 1000);
    setRemainingTime(getRemainingTime(auction.endTime));
    return () => clearInterval(interval);
  }, [auction]);

  const fetchAuction = async () => {
    const res = await GetAuctionPage(id);

    if (res.success && res.auctionPage) {
      setAuction(res.auctionPage);
    } else {
      setMessage(res.message || "Auction loading error");
    }
    setLoading(false);
  };

  useEffect(() => {
    if (id) {
      fetchAuction();
    }
  }, [id]);

  const fetchPhotos = async () => {
    if (!auction) return;
    const loadedPhotos: string[] = [];

    for (let i = 0; i <= 4; i++) {
      const res = await GetAuctionPhoto(auction.auctionId, i);
      if (!res.success || !res.imageUrl) break;
      loadedPhotos.push(res.imageUrl);
    }

    setPhotos(loadedPhotos);
  };

  useEffect(() => {
    if (auction) {
      fetchPhotos();
    }
  }, [auction]);

  useEffect(() => {
    let isMounted = true;

    const waitForConnected = async (connection: HubConnection) => {
      while (
        connection.state !== HubConnectionState.Connected &&
        connection.state !== HubConnectionState.Disconnected
      ) {
        await new Promise((res) => setTimeout(res, 100));
      }
    };

    const setupConnection = async () => {
      if (!connectionRef.current) {
        const connection = new HubConnectionBuilder()
          .withUrl("http://localhost:5005/auctionHub", {
            withCredentials: true,
          })
          .withAutomaticReconnect()
          .configureLogging(LogLevel.Information)
          .build();
        connectionRef.current = connection;
      }

      const connection = connectionRef.current;
      try {
        if (connection.state === HubConnectionState.Disconnected) {
          await connection.start();
          await waitForConnected(connection);
        } else {
          await waitForConnected(connection);
        }

        if (connection.state === HubConnectionState.Connected && isMounted) {
          connection.on(
            "ReceiveMessage",
            (userCategory: string, message: string, timestamp: string) => {
              setMessages((prev) => [
                ...prev,
                {
                  userCategory,
                  message,
                  timestamp: new Date(timestamp).getTime(),
                },
              ]);
            }
          );
          connection.on("ReceiveBidUpdate", () => {
            fetchAuction();
          });
          await connection.invoke("JoinAuctionGroup", id);
        }
      } catch (err) {
        if (isMounted) {
          setMessage(
            err instanceof Error
              ? err.message
              : "An error occurred while connecting"
          );
        }
      }
    };

    setupConnection();

    return () => {
      isMounted = false;
      connectionRef.current?.off("ReceiveBidUpdate");
      connectionRef.current?.off("ReceiveMessage");
    };
  }, [id]);

  useEffect(() => {
    const handleBeforeUnload = () => {
      const connection = connectionRef.current;
      if (
        connection &&
        connection.state === HubConnectionState.Connected &&
        id
      ) {
        connection.invoke("LeaveAuctionGroup", id).catch(() => {});
      }
    };

    window.addEventListener("beforeunload", handleBeforeUnload);
    return () => {
      window.removeEventListener("beforeunload", handleBeforeUnload);
    };
  }, [id]);

  useEffect(() => {
    return () => {
      const connection = connectionRef.current;
      if (
        connection &&
        connection.state === HubConnectionState.Connected &&
        id
      ) {
        connection
          .invoke("LeaveAuctionGroup", id)
          .catch(() => {})
          .finally(() => connection.stop());
      }
    };
  }, [id]);

  const handleCloseAuction = async (): Promise<void> => {
    try {
      const result = await CloseAuction(auction?.auctionId);
      if (!result.success) {
        setMessage(result.message || "Unknown server error");
      }
      if (result.success) {
        window.location.reload();
      }
    } catch (error) {
      setMessage("Error when closing the auction");
    }
  };
  const handleCompleteAuction = async (): Promise<void> => {
    try {
      const result = await CompleteAuctionDeal(auction?.auctionId);
      if (!result.success) {
        setMessage(result.message || "Unknown server error");
      }
      if (result.success) {
        window.location.reload();
      }
    } catch (error) {
      setMessage("Error when complete deal for auction");
    }
  };
  const handleAddPhotos = async (
    files: FileList | File[]
  ): Promise<{ success: boolean; message?: string }> => {
    if (!files || files.length === 0) {
      return { success: false, message: "No files to download" };
    }
    if (!auction?.auctionId) {
      return { success: false, message: "AuctionId is empty" };
    }

    const result = await AddAuctionPhotos(files, auction.auctionId);
    if (result.success) {
      await fetchPhotos();
    }
    return result;
  };

  const handleDeletePhotos = async (
    selectedIndex: number[]
  ): Promise<{ success: boolean; message?: string }> => {
    if (!auction?.auctionId) {
      return { success: false, message: "AuctionId is empty" };
    }

    const result = await DeleteAuctionPhotos(selectedIndex, auction.auctionId);
    if (result.success) {
      await fetchPhotos();
    }
    return result;
  };
  const handleCreateBid = async (
    amountBid: number
  ): Promise<{ success: boolean; message: string }> => {
    const connection = connectionRef.current;
    if (
      !connection ||
      connection.state !== HubConnectionState.Connected ||
      !auction?.auctionId
    ) {
      return {
        success: false,
        message: "No connection to the auction or auctionId is not loaded",
      };
    }

    try {
      const result: string = await connection.invoke(
        "CreateBid",
        auction.auctionId,
        amountBid
      );

      if (result === "Bid sent successfully") {
        return { success: true, message: "Bid sent successfully" };
      } else {
        return { success: false, message: result };
      }
    } catch (error) {
      return {
        success: false,
        message: "There was an error in creating the bid",
      };
    }
  };
  const handleContact = async () => {
    const result = await GetCreatorOrWinnerId(auction.auctionId);
    if (result.success && result.id) {
      router.push(`/user/${result.id}`);
    } else {
      setMessage(result.message || "Contact problems");
    }
  };

  const handleDeleteBid = async (): Promise<void> => {
    const connection = connectionRef.current;
    if (
      !connection ||
      connection.state !== HubConnectionState.Connected ||
      !auction?.auctionId
    ) {
      setMessage("No connection to the auction or auctionId is not loaded");
      return;
    }

    try {
      const result: string = await connection.invoke(
        "DeleteBid",
        auction.auctionId
      );
      if (result !== "Bid deleted successfully") {
        setMessage(result);
      }
    } catch (error) {
      setMessage("No connection to the auction or auctionId is not loaded");
    }
  };
  const handleSendMessage = async (msg: string) => {
    const connection = connectionRef.current;
    if (
      !connection ||
      connection.state !== HubConnectionState.Connected ||
      !auction?.auctionId
    ) {
      setMessage("Нет подключения к аукциону");
      return;
    }

    try {
      await connection.invoke("SendMessageAuctionChat", auction.auctionId, msg);
    } catch (error) {
      setMessage("Ошибка при отправке сообщения");
    }
  };

  if (loading) return <SpinItem />;
  if (message) {
    return (
      <div>
        <Title
          level={2}
          style={{
            color: "red",
            textAlign: "center",
            marginTop: 50,
          }}
        >
          {message || "Server error"}
        </Title>
      </div>
    );
  }

  if (!auction) return null;
  return (
    <>
      <div
        style={{
          maxWidth: 1200, // максимальная ширина всей страницы
          margin: "40px auto", // центрирование по горизонтали + отступ сверху и снизу
          padding: "0 20px", // отступы слева и справа
          boxSizing: "border-box",
        }}
      >
        <div
          style={{
            display: "flex",
            flexWrap: "wrap",
            justifyContent: "center",
            gap: "24px",
            alignItems: "flex-start",
            marginBottom: 40, // отступ между карточками и чатом
          }}
        >
          <AuctionPageCard
            auction={auction}
            photos={photos}
            remainingTime={remainingTime}
          />
          <AuctionButtonsCard
            userType={auction.type}
            auctionStatus={auction.status}
            photos={photos}
            handleCloseAuction={handleCloseAuction}
            handleCompleteAuction={handleCompleteAuction}
            handleAddPhotos={handleAddPhotos}
            handleDeletePhotos={handleDeletePhotos}
            handleCreatebid={handleCreateBid}
            handleDeleteBid={handleDeleteBid}
            handleContact={handleContact}
          />
        </div>

        {auction.status === "Active" && (
          <div style={{ display: "flex", justifyContent: "center" }}>
            <ChatComponent messages={messages} onSend={handleSendMessage} />
          </div>
        )}
      </div>
    </>
  );
};

export default AuctionPage;
