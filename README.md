# 🎓 3D SQL Learning 3D World – Unity Thesis Project

This project is a thesis implementation of a 3D learning environment for SQL education, developed in Unity and enriched with gamification elements, logging systems, checkpoint tracking, and AI NPC guidance using the OpenAI API.

---

## 📦 Contents

- [Requirements](#-requirements)
- [Installation Instructions](#-installation-instructions)
  - [Unity Project](#unity-project)
  - [Node.js Server](#nodejs-server)
  - [MySQL Database](#mysql-database)
- [Repository Structure](#-repository-structure)
- [Credits](#-credits)
- [License](#-license)

---

## 🔧 Requirements

- **Unity:** 2021.3 LTS or later
- **Node.js:** version 18 or later
- **MySQL:** version 8 or later
- **OpenAI API Key:** required for the AI NPC features

---

## 🛠 Installation Instructions

### 🎮 Unity Project

1. Clone the repository:
   ```bash
   git clone https://github.com/dimliopeta/thesis.git
   ```

2. Open the `UnityProject/` folder via Unity Hub.

3. Ensure the following packages are installed:
   - TextMesh Pro

---

### 🌐 Node.js Server

1. Navigate to the `server/` folder:
   ```bash
   cd server
   ```

2. Install dependencies:
   ```bash
   npm install
   ```

3. Create a `.env` file with the following content:
   ```env
   OPENAI_API_KEY=your_openai_api_key_here
   ```

   `OPENAI_API_KEY` is required to activate the AI NPC.
   You can generate a key at: [https://platform.openai.com/account/api-keys](https://platform.openai.com/account/api-keys)

4. Start the server:
   ```bash
   npm start
   ```

---

### 🗄️ MySQL Database

1. Create a new database:
   ```sql
   CREATE DATABASE sqlgame;
   ```

2. Execute the SQL schema:
   ```bash
   mysql -u root -p sqlgame < schema.sql
   ```

3. Make sure you change the `config.js` file values to match your MySQL credentials.

---

## 📁 Repository Structure

```
/
├── UnityProject/         # Main Unity game
│   └── Assets/           # Scripts, Scenes, Prefabs, etc.
├── server/               # Node.js backend
│   └── index.js, routes/, utils/, etc.
├── schema.sql            # SQL file for database setup
├── .gitignore
├── .gitattributes
└── README.md             # This file
```

---

## 👥 Credits

### 📢 Event System: `Messenger.cs`

Uses `Messenger.cs` for script-to-script messaging:

https://github.com/jhocking/from-unity-wiki/blob/main/Messenger.cs

---

### 🧍‍♂️ 3D Avatars

Character avatars from:

**Mini Simple Characters – Free Demo**  
🔗 https://assetstore.unity.com/packages/3d/characters/humanoids/mini-simple-characters-free-demo-262799

---

## ⚖️ License

This project was developed for academic and educational purposes.  
Any third-party assets or libraries used are subject to their respective licenses.

---

## ✉️ Contact

For questions or suggestions, reach out via GitHub or email.
