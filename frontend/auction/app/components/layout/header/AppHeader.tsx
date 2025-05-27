"use client";
import jwt from "jsonwebtoken";
import React, { useEffect, useState } from "react";
import Link from "next/link";
import { Layout, Menu } from "antd";
import type { MenuProps } from "antd";
import LoginModal from "./LoginModal";
import RegisterModal from "./RegisterModal";
import { getJwtUserId } from "../../../infrastructure/jwt";

import { Login, Register } from "../../../services/api";

const { Header } = Layout;

function AppHeader() {
  const [isModalLoginVisible, setIsModalLoginVisible] = useState(false);
  const [isModalRegisterVisible, setIsModalRegisterVisible] = useState(false);
  const [isAuthenticated, setIsAuthenticated] = useState(false);
  const [jwtUserId, setJwtUserId] = useState<string | null>(null);
  const [loading, setLoading] = useState(true);

  useEffect(() => {
    const userId = getJwtUserId();
    setTimeout(() => {
      setJwtUserId(userId);
      setIsAuthenticated(Boolean(userId));
      setLoading(false);
    }, 200);
  }, []);

  const showLoginModal = () => setIsModalLoginVisible(true);
  const showRegisterModal = () => setIsModalRegisterVisible(true);

  const handleLogin = async (
    email: string,
    password: string
  ): Promise<{ success: boolean; message?: string }> => {
    try {
      const result = await Login(email, password);
      if (result.success) {
        setIsAuthenticated(true);
      }
      return result;
    } catch (error) {
      return { success: false, message: "Login error" };
    }
  };

  const handleRegister = async (
    userName: string,
    email: string,
    password: string,
    contacts: string
  ): Promise<{ success: boolean; message?: string }> => {
    try {
      return await Register(userName, email, password, contacts);
    } catch (error) {
      return { success: false, message: "Registration error" };
    }
  };

  const handleCancel = () => {
    setIsModalRegisterVisible(false);
    setIsModalLoginVisible(false);
    window.location.reload();
  };

  const handleLogout = () => {
    document.cookie = "MyCookies=; Max-Age=0; path=/;";
    setIsAuthenticated(false);
    window.location.reload();
  };

  if (loading) {
    return (
      <Header
        style={{
          backgroundColor: "#001529",
          display: "flex",
          justifyContent: "center",
          alignItems: "center",
          padding: 0,
          height: 80,
          fontSize: 16,
        }}
      >
        <div style={{ color: "white" }}>Загрузка...</div>
      </Header>
    );
  }

  const menuItems: MenuProps["items"] = [
    {
      key: "1",
      label: (
        <Link href="/" style={{ fontSize: 16 }}>
          Home
        </Link>
      ),
    },
  ];

  const rightMenuItems: MenuProps["items"] = isAuthenticated
    ? [
        {
          key: "2",
          label: (
            <Link href={`/user/${jwtUserId}`} style={{ fontSize: 16 }}>
              My profile
            </Link>
          ),
        },
        {
          key: "3",
          label: (
            <div
              onClick={handleLogout}
              style={{ cursor: "pointer", fontSize: 16 }}
            >
              Logout
            </div>
          ),
        },
      ]
    : [
        {
          key: "2",
          label: (
            <div
              onClick={showRegisterModal}
              style={{ cursor: "pointer", fontSize: 18 }}
            >
              Register
            </div>
          ),
        },
        {
          key: "3",
          label: (
            <div
              onClick={showLoginModal}
              style={{ cursor: "pointer", fontSize: 16 }}
            >
              Login
            </div>
          ),
        },
      ];

  return (
    <Header
      style={{
        position: "fixed",
        top: 0,
        left: 0,
        width: "100%",
        zIndex: 1000,
        backgroundColor: "#001529",
        display: "flex",
        justifyContent: "space-between",
        alignItems: "center",
        padding: "0 24px",
        height: 80,
      }}
    >
      <Menu
        theme="dark"
        mode="horizontal"
        items={menuItems}
        selectedKeys={[]}
        style={{ flex: 1 }}
      />
      <Menu
        theme="dark"
        mode="horizontal"
        items={rightMenuItems}
        selectedKeys={[]}
        style={{ flex: 1, justifyContent: "flex-end" }}
      />
      <LoginModal
        isModalVisible={isModalLoginVisible}
        handleLogin={handleLogin}
        handleCancel={handleCancel}
      />
      <RegisterModal
        isModalVisible={isModalRegisterVisible}
        handleRegister={handleRegister}
        handleCancel={handleCancel}
      />
    </Header>
  );
}

export default AppHeader;
