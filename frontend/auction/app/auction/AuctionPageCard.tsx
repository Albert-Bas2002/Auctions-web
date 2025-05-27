"use client";
import { Carousel, Typography } from "antd";
import Card from "antd/es/card/Card";
import Image from "next/image";
import { AuctionPageDto } from "../services/api";

const { Title, Paragraph } = Typography;

interface AuctionPageCardProps {
  auction: AuctionPageDto;
  photos: string[];
  remainingTime: string;
}

export default function AuctionPageCard({
  auction,
  photos,
  remainingTime,
}: AuctionPageCardProps) {
  return (
    <div>
      <Card
        style={{
          margin: 10,
          maxWidth: 800,
          marginLeft: "auto",
          marginRight: "auto",
          padding: 10,
          paddingTop: 24,
        }}
      >
        <Title
          level={2}
          style={{
            textAlign: "center",
            marginBottom: 16,
            marginTop: 0,
          }}
        >
          {auction.title}
        </Title>

        <div
          style={{
            display: "flex",
            gap: 24,
            alignItems: "flex-start",
            flexWrap: "wrap",
            justifyContent: "center",
          }}
        >
          {photos.length > 0 && (
            <div
              style={{
                flex: "1 1 400px",
                minWidth: 280,
                maxWidth: 500,
              }}
            >
              <Carousel autoplay arrows>
                {photos.map((url, index) => (
                  <Image
                    key={index}
                    src={url}
                    alt={`Фото ${index + 1}`}
                    width={400}
                    height={300}
                    style={{ objectFit: "cover", borderRadius: 8 }}
                  />
                ))}
              </Carousel>
            </div>
          )}

          <div
            style={{
              flex: "1 1 250px",
              minWidth: 250,
              maxWidth: 500,
              wordBreak: "break-word",
            }}
          >
            <Paragraph>
              <strong>Status:</strong> {auction.status}
            </Paragraph>
            {auction.type !== "Winner" &&
              auction.status === "Active" &&
              auction.currentBid != null &&
              auction.currentBid !== 0 &&
              auction.biddersBid !== auction.currentBid && (
                <Paragraph>
                  <strong>Current bid:</strong> ${auction.currentBid}
                </Paragraph>
              )}

            {auction.status !== "Active" && auction.type !== "Winner" && (
              <Paragraph>
                <strong>Last bid:</strong> ${auction.currentBid}
              </Paragraph>
            )}
            <Paragraph>
              <strong>Reserve:</strong> ${auction.reserve}
            </Paragraph>

            {auction.biddersBid && (
              <Paragraph>
                <strong>
                  {auction.type === "Winner"
                    ? "The bid with which you won:"
                    : auction.biddersBid === auction.currentBid
                    ? "Your bid is now in the lead:"
                    : "Your bid:"}
                </strong>{" "}
                ${auction.biddersBid}
              </Paragraph>
            )}
            <Paragraph>
              <strong>Create at:</strong>{" "}
              {new Date(auction.creationTime).toLocaleString()}
            </Paragraph>
            <Paragraph>
              <strong>Ending:</strong> {remainingTime}
            </Paragraph>
          </div>
        </div>

        <div
          style={{
            marginTop: 24,
            maxWidth: 800,
            marginLeft: "auto",
            marginRight: "auto",
            textAlign: "center",
            wordBreak: "break-word",
          }}
        >
          <Paragraph>
            <strong>Description:</strong> {auction.description}
          </Paragraph>
        </div>
      </Card>
    </div>
  );
}
