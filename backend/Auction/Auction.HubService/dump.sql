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
-- Name: AuctionChatMessages; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."AuctionChatMessages" (
    "MessageId" uuid NOT NULL,
    "AuctionId" uuid NOT NULL,
    "SenderId" uuid NOT NULL,
    "Timestamp" timestamp with time zone NOT NULL,
    "UserCategoryForAuction" character varying(30) NOT NULL,
    "Message" character varying(200) NOT NULL
);


ALTER TABLE public."AuctionChatMessages" OWNER TO postgres;

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public; Owner: postgres
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);


ALTER TABLE public."__EFMigrationsHistory" OWNER TO postgres;

--
-- Data for Name: AuctionChatMessages; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."AuctionChatMessages" ("MessageId", "AuctionId", "SenderId", "Timestamp", "UserCategoryForAuction", "Message") FROM stdin;
\.


--
-- Data for Name: __EFMigrationsHistory; Type: TABLE DATA; Schema: public; Owner: postgres
--

COPY public."__EFMigrationsHistory" ("MigrationId", "ProductVersion") FROM stdin;
20250523180204_init	9.0.4
\.


--
-- Name: AuctionChatMessages PK_AuctionChatMessages; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."AuctionChatMessages"
    ADD CONSTRAINT "PK_AuctionChatMessages" PRIMARY KEY ("MessageId");


--
-- Name: __EFMigrationsHistory PK___EFMigrationsHistory; Type: CONSTRAINT; Schema: public; Owner: postgres
--

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");


--
-- PostgreSQL database dump complete
--

