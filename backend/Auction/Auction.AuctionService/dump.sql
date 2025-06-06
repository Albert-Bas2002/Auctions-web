--
-- PostgreSQL database dump
--

-- Dumped from database version 17.5 (Debian 17.5-1.pgdg120+1)
-- Dumped by pg_dump version 17.5 (Debian 17.5-1.pgdg120+1)

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET transaction_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

SET default_tablespace = '';

SET default_table_access_method = heap;

--
-- Name: AuctionsDetails; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AuctionsDetails" (
    "AuctionId" uuid NOT NULL,
    "AuctionCreatorId" uuid NOT NULL,
    "Title" character varying(100) NOT NULL,
    "Description" character varying(1000) NOT NULL,
    "CreationTime" timestamp with time zone NOT NULL,
    "EndTime" timestamp with time zone NOT NULL,
    "IsActive" boolean NOT NULL,
    "Reserve" integer DEFAULT 0 NOT NULL
);


ALTER TABLE public."AuctionsDetails" OWNER TO postgres;

--
-- Name: AuctionsStatus; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AuctionsStatus" (
    "AuctionId" uuid NOT NULL,
    "IsCloseByCreator" boolean NOT NULL,
    "IsCloseByModerator" boolean NOT NULL,
    "HasAuctionWinner" boolean NOT NULL,
    "AuctionWinnerId" uuid NOT NULL,
    "IsDealCompletedByAuctionWinner" boolean NOT NULL,
    "IsDealCompletedByAuctionCreator" boolean NOT NULL,
    "IsCompletelyFinished" boolean NOT NULL
);


ALTER TABLE public."AuctionsStatus" OWNER TO postgres;

--
-- Name: Bids; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."Bids" (
    "BidId" uuid NOT NULL,
    "BidCreatorId" uuid NOT NULL,
    "AuctionId" uuid NOT NULL,
    "Value" integer NOT NULL,
    "CreationTime" timestamp with time zone NOT NULL
);


ALTER TABLE public."Bids" OWNER TO postgres;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Data for Name: AuctionsDetails; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AuctionsDetails" ("AuctionId", "AuctionCreatorId", "Title", "Description", "CreationTime", "EndTime", "IsActive", "Reserve") FROM stdin;
\.


--
-- Data for Name: AuctionsStatus; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AuctionsStatus" ("AuctionId", "IsCloseByCreator", "IsCloseByModerator", "HasAuctionWinner", "AuctionWinnerId", "IsDealCompletedByAuctionWinner", "IsDealCompletedByAuctionCreator", "IsCompletelyFinished") FROM stdin;
\.


--
-- Data for Name: Bids; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."Bids" ("BidId", "BidCreatorId", "AuctionId", "Value", "CreationTime") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250523175153_init	9.0.4
\.


--
-- Name: AuctionsDetails PK_AuctionsDetails; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AuctionsDetails"
    ADD CONSTRAINT "PK_AuctionsDetails" PRIMARY KEY ("AuctionId");


--
-- Name: AuctionsStatus PK_AuctionsStatus; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AuctionsStatus"
    ADD CONSTRAINT "PK_AuctionsStatus" PRIMARY KEY ("AuctionId");


--
-- Name: Bids PK_Bids; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Bids"
    ADD CONSTRAINT "PK_Bids" PRIMARY KEY ("BidId");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- Name: IX_Bids_AuctionId; Type: INDEX; Schema: public; Owner: postgres
--

CREATE INDEX "IX_Bids_AuctionId" ON public."Bids" USING btree ("AuctionId");


--
-- Name: AuctionsStatus FK_AuctionsStatus_AuctionsDetails_AuctionId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AuctionsStatus"
    ADD CONSTRAINT "FK_AuctionsStatus_AuctionsDetails_AuctionId" FOREIGN KEY ("AuctionId") REFERENCES public."AuctionsDetails"("AuctionId") ON DELETE CASCADE;


--
-- Name: Bids FK_Bids_AuctionsDetails_AuctionId; Type: FK CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."Bids"
    ADD CONSTRAINT "FK_Bids_AuctionsDetails_AuctionId" FOREIGN KEY ("AuctionId") REFERENCES public."AuctionsDetails"("AuctionId") ON DELETE CASCADE;


--
-- PostgreSQL database dump complete
--

