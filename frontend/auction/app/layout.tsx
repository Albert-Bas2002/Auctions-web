"use client";
import React from "react";
import { Layout } from "antd";
import AppHeader from "./components/layout/header/AppHeader";
import AppFooter from "./components/layout/header/AppFooter";

export default function RootLayout({
  children,
}: {
  children: React.ReactNode;
}) {
  return (
    <html lang="en">
      <body style={{ margin: 0, padding: 0 }}>
        <Layout>
          <AppHeader />
          <Layout.Content
            style={{ paddingTop: 80, minHeight: "calc(100vh - 64px - 70px)" }}
          >
            {children}
          </Layout.Content>
          <AppFooter />
        </Layout>
      </body>
    </html>
  );
}
