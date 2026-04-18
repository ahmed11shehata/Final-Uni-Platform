import { useState } from "react";
import { Outlet } from "react-router-dom";
import { motion } from "framer-motion";
import Sidebar from "../common/Sidebar";
import Topbar from "./Topbar";
import styles from "./MainLayout.module.css";

export default function MainLayout() {
  const [collapsed, setCollapsed] = useState(false);

  return (
    <div className={styles.layout}>
      <Sidebar collapsed={collapsed} onToggle={() => setCollapsed(p => !p)} />
      <motion.div
        className={styles.main}
        animate={{ marginLeft: collapsed ? 72 : 240 }}
        transition={{ duration: 0.3, ease: [0.4, 0, 0.2, 1] }}
      >
        <Topbar />
        <main className={styles.content}>
          <motion.div
            initial={{ opacity: 0, y: 16 }}
            animate={{ opacity: 1, y: 0 }}
            transition={{ duration: 0.4, ease: "easeOut" }}
          >
            <Outlet />
          </motion.div>
        </main>
      </motion.div>
    </div>
  );
}