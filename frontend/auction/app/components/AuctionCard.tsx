"use client";
import { Col } from "antd";
import Image from "next/image";
import { useRouter } from "next/navigation";
import dayjs from "dayjs";
import duration from "dayjs/plugin/duration";
import utc from "dayjs/plugin/utc";
import Card from "antd/es/card/Card";

dayjs.extend(duration);
dayjs.extend(utc);

interface AuctionCardProps {
  auction: {
    auctionId: string;
    title: string;
    currentBid: number;
    reserve: number;
    endTime: string;
  };
  imageUrl: string;
}

function getRemainingTime(endTime: string) {
  const now = dayjs();
  const end = dayjs(endTime);
  const diff = end.diff(now);

  if (diff <= 0) return "Ended";

  const d = dayjs.duration(diff);

  if (d.asDays() >= 1) {
    return `${Math.floor(d.asDays())} day${
      Math.floor(d.asDays()) > 1 ? "s" : ""
    } left`;
  }

  const hours = String(d.hours()).padStart(2, "0");
  const minutes = String(d.minutes()).padStart(2, "0");
  const seconds = String(d.seconds()).padStart(2, "0");

  return `${hours}:${minutes}:${seconds}`;
}

export default function AuctionCard({ auction, imageUrl }: AuctionCardProps) {
  const router = useRouter();

  return (
    <Col key={auction.auctionId} xs={12} sm={10} md={8} lg={6}>
      <Card
        hoverable
        style={{
          display: "flex",
          flexDirection: "column",
          borderRadius: 12,
          boxShadow: "0 4px 10px rgba(0,0,0,0.1)",
          transition: "transform 0.3s ease",
          backgroundColor: "#fff",
        }}
        onClick={() => router.push(`/auction/${auction.auctionId}`)}
        onMouseEnter={(e: React.MouseEvent<HTMLDivElement>) =>
          (e.currentTarget.style.transform = "scale(1.05)")
        }
        onMouseLeave={(e: React.MouseEvent<HTMLDivElement>) =>
          (e.currentTarget.style.transform = "scale(1)")
        }
      >
        <div
          style={{
            flexShrink: 0,
            width: "100%",
            aspectRatio: "4 / 3",
            marginBottom: 12,
            borderRadius: 12,
            overflow: "hidden",
            backgroundColor: "#fff",
            display: "flex",
            alignItems: "center",
            justifyContent: "center",
          }}
        >
          <Image
            src={imageUrl || "/default-auction.jpg"}
            alt="auction"
            width={300}
            height={225}
            style={{
              objectFit: "contain",
              width: "100%",
              height: "100%",
            }}
          />
        </div>

        <div>
          <h3
            style={{
              marginBottom: 8,
              fontSize: 18,
              fontWeight: 600,
              color: "#222",
              wordWrap: "break-word",
              whiteSpace: "normal",
              overflowWrap: "break-word",
            }}
          >
            {auction.title}
          </h3>

          <p style={{ margin: "4px 0", color: "#555", fontSize: 14 }}>
            <b>Current Bid:</b> ${auction.currentBid}
          </p>
          <p style={{ margin: "4px 0", color: "#555", fontSize: 14 }}>
            <b>Reserve:</b> ${auction.reserve}
          </p>
          <p
            style={{
              marginTop: 12,
              fontWeight: 600,
              color:
                getRemainingTime(auction.endTime) === "Ended"
                  ? "#000"
                  : auction.currentBid >= auction.reserve
                  ? "#2f855a"
                  : "#d69e2e",
              fontSize: 14,
            }}
          >
            {getRemainingTime(auction.endTime) === "Ended"
              ? "Ended"
              : `Ends: ${getRemainingTime(auction.endTime)}`}
          </p>
        </div>
      </Card>
    </Col>
  );
}
