"use client";
import { Row, Button, Spin, Typography } from "antd";
import { useEffect, useState } from "react";
import dayjs from "dayjs";
import duration from "dayjs/plugin/duration";
import utc from "dayjs/plugin/utc";
const { Title, Text } = Typography;
import { GetAuctions } from "../services/api";
import { Select } from "antd";
import { GetAuctionPhoto } from "../services/api";
import AuctionCard from "./AuctionCard";
import SpinItem from "./SpinItem";

const { Option } = Select;
dayjs.extend(duration);
dayjs.extend(utc);

interface AuctionListItem {
  auctionId: string;
  creationTime: string;
  endTime: string;
  title: string;
  reserve: number;
  currentBid: number;
}

export default function Auctions() {
  const [auctions, setAuctions] = useState<AuctionListItem[]>([]);
  const [page, setPage] = useState(1);
  const [loading, setLoading] = useState(false);
  const [initialLoading, setInitialLoading] = useState(true);
  const [message, setMessage] = useState<string>("");
  const [sort, setSort] = useState("creationtime_desc");
  const [imageMap, setImageMap] = useState<Record<string, string>>({});

  useEffect(() => {
    loadAuctions(page);
  }, [page]);
  useEffect(() => {
    setAuctions([]);
  }, [sort]);
  useEffect(() => {
    async function fetchImages() {
      const newImageMap: Record<string, string> = { ...imageMap };

      for (const auction of auctions) {
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

    if (auctions.length > 0) {
      fetchImages();
    }
  }, [auctions]);

  const handleSortChange = (value: string) => {
    setSort(value);
    setPage(1);
    setAuctions([]);
    loadAuctions(1, value);
  };
  const loadAuctions = async (pageToLoad: number, newSort?: string) => {
    if (pageToLoad === 1) setInitialLoading(true);
    else setLoading(true);

    const sortParam = newSort ?? sort;

    const result = await GetAuctions(pageToLoad, sortParam);

    if (result === null || result === undefined) {
      setMessage("Server error");
    } else if (!result.success) {
      setMessage(result.message ?? "Unknown server error");
    } else {
      if (pageToLoad === 1) {
        setAuctions(result.auctionListItems ?? []);
      } else {
        setAuctions((prev) => [...prev, ...(result.auctionListItems ?? [])]);
      }
    }

    if (pageToLoad === 1) setInitialLoading(false);
    else setLoading(false);
  };

  if (initialLoading) {
    return <SpinItem />;
  }
  if (message) {
    const isNoAuctionsMessage =
      message === "There are no auctions at this time";
    return (
      <div>
        <Title
          level={2}
          style={{
            color: isNoAuctionsMessage ? "black" : "red",
            textAlign: "center",
            marginTop: 50,
          }}
        >
          {message || "No auctions found"}
        </Title>
      </div>
    );
  }
  return (
    <div style={{ padding: "24px", maxWidth: 1200, margin: "0 auto" }}>
      <div
        style={{
          display: "flex",
          justifyContent: "space-between",
          alignItems: "center",
          marginBottom: 24,
        }}
      >
        <h1 style={{ fontSize: "28px", fontWeight: "600" }}>Auctions</h1>
        <Select
          value={sort}
          onChange={handleSortChange}
          style={{ width: 220 }}
          size="middle"
          options={[
            { value: "creationtime_desc", label: "Newest First" },
            { value: "creationtime_asc", label: "Oldest First" },
            { value: "endtime_asc", label: "Ending Soon" },
            { value: "endtime_desc", label: "Ending Last" },
            { value: "reserve_asc", label: "Lowest Reserve" },
            { value: "reserve_desc", label: "Highest Reserve" },
          ]}
        />
      </div>
      <h1
        style={{
          fontSize: "28px",
          marginBottom: "24px",
          textAlign: "center",
          fontWeight: "600",
        }}
      ></h1>
      <Row gutter={[24, 24]}>
        {auctions.map((auction) => (
          <AuctionCard
            key={auction.auctionId}
            auction={auction}
            imageUrl={imageMap[auction.auctionId] || "/default-auction.jpg"}
          />
        ))}
      </Row>

      <div style={{ textAlign: "center", marginTop: 24 }}>
        <Button
          onClick={() => setPage((prev) => prev + 1)}
          loading={loading}
          type="primary"
        >
          Load More
        </Button>
      </div>
    </div>
  );
}
