require("dotenv").config();
const express = require("express");
const db = require("./config");
const jwt = require("jsonwebtoken");
const bcrypt = require("bcrypt");
const cors = require("cors");
const cookieParser = require("cookie-parser");
const { v4: uuidv4 } = require('uuid');

const path = require('path');
const app = express();
const bodyParser = require('body-parser');


app.use(bodyParser.json());
app.use(bodyParser.urlencoded({ extended: true }));
app.use(express.json());
app.use(express.urlencoded({ extended: true }));
app.use(cors());
app.use(cookieParser());

const PORT = process.env.PORT || 3000;
const SECRET_KEY = process.env.JWT_SECRET || "your-secret-key";

app.use(express.static(path.join(__dirname, 'public')));


// **Login API**
app.post("/login/web", async (req, res) => {
    const { username, password } = req.body;

    const query = "SELECT * FROM users WHERE username = ?";
    db.query(query, [username], async (err, results) => {
        if (err) {
            return res.status(500).json({ error: "Database error", details: err });
        }

        if (results.length === 0) {
            return res.status(400).json({ error: "Invalid credentials" });
        }

        const user = results[0];
        const validPassword = await bcrypt.compare(password, user.password);

        if (!validPassword) {
            return res.status(400).json({ error: "Invalid credentials" });
        }

        // Επιτρέπουμε ΜΟΝΟ στους καθηγητές να συνδεθούν στην ιστοσελίδα
        if (user.role !== "teacher") {
            return res.status(403).json({ error: "Access denied" });
        }

        const token = jwt.sign({ id: user.id, role: user.role }, SECRET_KEY, { expiresIn: "1h" });
        res.cookie('token', token, { httpOnly: true });
        return res.redirect('/dashboard');
    });
});

app.post("/login/game", async (req, res) => {
    const { username, password } = req.body;

    const query = "SELECT * FROM users WHERE username = ?";
    db.query(query, [username], async (err, results) => {
        if (err) return res.status(500).json({ error: "Database error", details: err });
        if (results.length === 0) return res.status(400).json({ error: "Invalid credentials" });

        const user = results[0];
        const validPassword = await bcrypt.compare(password, user.password);
        if (!validPassword) return res.status(400).json({ error: "Invalid credentials" });
        if (user.role !== "student") return res.status(403).json({ error: "Access denied" });

        const token = jwt.sign({ id: user.id, role: user.role }, SECRET_KEY, { expiresIn: "1h" });

        // Δημιουργία session
        const sessionId = uuidv4();
        const sessionQuery = "INSERT INTO sessions (id, user_id) VALUES (?, ?)";
        db.query(sessionQuery, [sessionId, user.id], (sessionErr) => {
            if (sessionErr) {
                console.error("Session creation error:", sessionErr);
                return res.status(500).json({ error: "Failed to create session" });
            }

            return res.json({ token, role: user.role, userId: user.id, sessionId });
        });
    });
});

app.get("/login/dev", async (req, res) => {
    const username = "testuser";  // στάνταρ test user
    const query = "SELECT * FROM users WHERE username = ?";
    db.query(query, [username], async (err, results) => {
        if (err) return res.status(500).json({ error: "Database error", details: err });
        if (results.length === 0) return res.status(400).json({ error: "Test user not found" });

        const user = results[0];
        const token = jwt.sign({ id: user.id, role: user.role }, SECRET_KEY, { expiresIn: "1h" });

        const sessionId = require('uuid').v4();
        const sessionQuery = "INSERT INTO sessions (id, user_id) VALUES (?, ?)";
        db.query(sessionQuery, [sessionId, user.id], (sessionErr) => {
            if (sessionErr) {
                console.error("Session creation error:", sessionErr);
                return res.status(500).json({ error: "Failed to create session" });
            }

            return res.json({ token, role: user.role, userId: user.id, sessionId });
        });
    });
});


const authenticateJWT = (req, res, next) => {
    const token = req.cookies?.token || req.headers.authorization?.split(' ')[1]; // Extract token from header or cookie - in this case, from the cookie 

    if (!token) {
        console.log('No token found.');
        return res.redirect('/login');
    }

    jwt.verify(token, SECRET_KEY, (err, user) => {
        if (err) {
            console.log('Token not verified.');
            return res.redirect('/login');
        }
        req.user = user;
        next();
    });
};
const authorizeRole = (requiredRole) => {
    return (req, res, next) => {
        if (req.user.role !== requiredRole) {
            return res.status(403).send('Access denied');
        }
        next();
    };
};

// Παράδειγμα προστατευμένης διαδρομής
app.get('/dashboard', authenticateJWT, authorizeRole('teacher'), (req, res) => {
    res.sendFile(path.join(__dirname, 'protected_views', 'dashboard.html'));
});


app.post('/logout', (req, res) => {
    // Clear existing cookie
    res.clearCookie('token', { httpOnly: true });
    // Return to index
    res.redirect('/');
});

app.post("/progress", authenticateJWT, (req, res) => {
    const { type, sessionId, userId, data, level } = req.body;
    const logId = uuidv4();

    console.log("Received log:", { type, sessionId, userId, data, level });

    const logQuery = `
        INSERT INTO logs (id, type, session_id, user_id, data, level)
        VALUES (?, ?, ?, ?, ?, ?)
    `;

    // Καταχώρηση στο logs
    db.query(logQuery, [logId, type, sessionId, userId, JSON.stringify(data), level], (err) => {
        if (err) {
            console.error("Log insert failed:", err);
            return res.status(500).json({ error: "Failed to log progress" });
        }

        console.log(`Progress log saved: ${logId}`);

        // SCORE_UPDATED αποθήκευση στο scores
        if (type === "SCORE_UPDATED") {
            let parsed;
            try {
                parsed = JSON.parse(data); 
            } catch (e) {
                console.error("Failed to parse SCORE_UPDATED data:", e);
                return res.status(400).json({ error: "Invalid score data" });
            }

            const insertScoreQuery = `
                INSERT INTO scores (user_id, session_id, game_name, score)
                VALUES (?, ?, ?, ?)
            `;

            db.query(
                insertScoreQuery,
                [userId, sessionId, parsed.game, parsed.scoreDelta],
                (scoreErr) => {
                    if (scoreErr) {
                        console.error("Score insert failed:", scoreErr);
                        return res.status(500).json({ error: "Failed to insert score" });
                    }

                    console.log("Score recorded in scores table.");
                    return res.json({ success: true });
                }
            );
        } else {
            return res.json({ success: true });
        }
    });
});





// ENDPOINT ΓΙΑ ΝΑ ΛΑΜΒΆΝΟΥΜΕ ΤΗΝ ΠΡΟΟΔΟ ΤΟΥ ΠΑΊΚΤΗ ΣΤΟ ΕΠΊΠΕΔΟ ΩΣΤΕ ΝΑ ΟΤΑΝ ΚΑΝΕΙ LOG-IN NA ΣΥΝΕΧΙΖΕΙ ΕΚΕΙ ΟΠΟΥ ΣΤΑΜΆΤΗΣΕ
app.get("/get-latest-log/:userId", (req, res) => {
    const userId = req.params.userId;

    const query = `
        SELECT type, data, level
        FROM logs
        WHERE user_id = ?
        ORDER BY created_at DESC
        LIMIT 1
    `;

    db.query(query, [userId], (err, results, fields) => {
        if (err) {
            console.error("Error getting latest log:", err);
            return res.status(500).json({ error: "Failed to get latest log" });
        }

        if (!results || results.length === 0) {
            return res.json({ state: "NEW" });
        }

        const latestLog = results[0];
        try {
            parsedData = JSON.parse(JSON.parse(latestLog.data));
        } catch (e) {
            console.error("Failed to parse data:", e);
        }

        const response = {
            type: latestLog.type,
            level: latestLog.level,
            book: parsedData.book || "",
            cube: parsedData.cube || "",
            query: parsedData.query || ""
        };

        res.json(response);
    });
});




// ------ NPC MESSAGE API -----------

const OpenAI = require("openai");
const openai = new OpenAI({ apiKey: process.env.OPENAI_API_KEY });

const TOKEN_LIMIT = 4000;
const APPROX_CHAR_PER_TOKEN = 4;
const MAX_CHAR_LENGTH = TOKEN_LIMIT * APPROX_CHAR_PER_TOKEN;

app.post("/npc-message", async (req, res) => {
  const {
    playerMessage,
    sessionId,
    userId,
    checkpoint,
    cheatsRemaining,
    game,
    gameContextJson
  } = req.body;

  console.log("Received NPC request: ", req.body);

  if (!playerMessage || !sessionId || !userId) {
    return res.status(400).json({ error: "Missing fields" });
  }

  try {
    // 1. Αποθήκευση μηνύματος χρήστη
    const firstMessageId = await insertMessage(userId, sessionId, 'user', playerMessage);

    // 2. Λήψη ιστορικού
    const chatHistory = await getSessionMessages(sessionId);
    const formattedMessages = buildChatHistory(chatHistory);
    const trimmedMessages = trimMessagesToFit(formattedMessages);

    // 3. Δημιουργία system prompt
    let basePrompt = `Είσαι ένας βοηθητικός NPC σε παιχνίδι εξάσκησης SQL.`;
    
    if (checkpoint) {
      basePrompt += `\nΟ παίκτης βρίσκεται στο checkpoint: ${checkpoint}.`;
    }
    
    if (checkpoint && game && gameContextJson) {
      basePrompt += `\nΤου απομένουν ${cheatsRemaining ?? "?"} διαθέσιμα cheat(s).`;
    }
    
    basePrompt += `
    Σου στέλνουμε το υπάρχον ιστορικό συνομιλίας σας.
    Θα απαντάς μόνο ερωτήσεις πάνω σε SQL. Αν η ερώτηση δεν σχετίζεται με SQL, απάντα ευγενικά ότι δεν μπορείς να βοηθήσεις.`;
    
    let gamePrompt = "";
    if (checkpoint && game && gameContextJson) {
      gamePrompt = `\nΟ παίκτης συμμετέχει στο mini-game "${game}". 
    Παρακάτω είναι το context του παιχνιδιού (σε JSON):
    ${gameContextJson}
    Αν ο παίκτης σου ζητήσει τη λύση, μπορείς να του τη δώσεις – αλλά μόνο μία φορά ανά ερώτημα. Η απάντηση σου θα ξεκινάει με "Η λύση είναι ". 
    Αφού του δώσεις τη σωστή απάντηση, ρώτησέ τον ευγενικά αν θέλει και εξήγηση.`;
    }
    
    const messages = [
      { role: "system", content: `${basePrompt}${gamePrompt}` },
      ...trimmedMessages
    ];
    

    // 4. OpenAI
    const completion = await openai.chat.completions.create({
      model: "gpt-3.5-turbo",
      messages: messages,
      temperature: 0.7
    });

    const npcReply = completion.choices[0].message.content.trim();
    let updatedCheatsRemaining = cheatsRemaining;

    // Αν είναι παιχνίδι και δόθηκε λύση, κάνε increment στον server
    if (game && npcReply.toLowerCase().startsWith("η λύση είναι")) {
      const cheatResult = await incrementCheat(sessionId, userId, game);
      updatedCheatsRemaining = cheatResult.remaining;

      // Log χρήση cheat
      const logQuery = `
        INSERT INTO logs (id, type, session_id, user_id, data, created_at, level)
        VALUES (UUID(), 'CHEAT_USED', ?, ?, ?, NOW(), 0)
      `;
      const logData = {
        message: playerMessage,
        reply: npcReply,
        game
      };

      db.query(logQuery, [sessionId, userId, JSON.stringify(logData)], (err) => {
        if (err) console.error("Failed to insert cheat log:", err);
      });
    }
    // 5. Αποθήκευση απάντησης NPC
    await insertMessage(userId, sessionId, 'npc', npcReply);

    // 6. Απάντηση προς Unity
    return res.json({
      reply: npcReply,
      messageId: firstMessageId,
      cheatsRemaining: updatedCheatsRemaining
    });
    

  } catch (err) {
    console.error("NPC Chat error:", err);
    return res.status(500).json({ error: "AI error" });
  }
});



// --- Helpers---

function insertMessage(userId, sessionId, sender, content) {
  return new Promise((resolve, reject) => {
    const query = `INSERT INTO messages (user_id, session_id, sender, content) VALUES (?, ?, ?, ?)`;
    db.query(query, [userId, sessionId, sender, content], (err, result) => {
      if (err) return reject(err);
      resolve(result.insertId); 
    });
  });
}


function getSessionMessages(sessionId) {
  return new Promise((resolve, reject) => {
    const query = `SELECT sender, content FROM messages WHERE session_id = ? ORDER BY timestamp ASC`;
    db.query(query, [sessionId], (err, results) => {
      if (err) return reject(err);
      resolve(results);
    });
  });
}

function buildChatHistory(dbMessages) {
  return dbMessages.map(row => ({
    role: row.sender === "npc" ? "assistant" : "user",
    content: row.content
  }));
}

function trimMessagesToFit(messages) {
  let totalChars = messages.reduce((sum, m) => sum + m.content.length, 0);
  if (totalChars <= MAX_CHAR_LENGTH) return messages;

  const trimmed = [...messages];
  while (trimmed.length > 1 && totalChars > MAX_CHAR_LENGTH) {
    const removed = trimmed.shift();
    totalChars -= removed.content.length;
  }
  return trimmed;
}

function incrementCheat(sessionId, userId, game) {
  return new Promise((resolve, reject) => {
    const updateCheat = `
      INSERT INTO cheats (session_id, game, cheats_used)
      VALUES (?, ?, 1)
      ON DUPLICATE KEY UPDATE cheats_used = cheats_used + 1;
    `;

    db.query(updateCheat, [sessionId, game], (err) => {
      if (err) return reject(err);

      const log = `
        INSERT INTO logs (id, type, session_id, user_id, data, created_at, level)
        VALUES (UUID(), 'CHEAT_USED', ?, ?, ?, NOW(), 1);
      `;
      const logData = JSON.stringify({
        game,
        reason: "User requested solution"
      });

      db.query(log, [sessionId, userId, logData], (err2) => {
        if (err2) return reject(err2);

        const select = `SELECT cheats_used FROM cheats WHERE session_id = ? AND game = ?`;
        db.query(select, [sessionId, game], (err3, results) => {
          if (err3) return reject(err3);
          const used = results[0]?.cheats_used ?? 0;
          const limit = 3;
          resolve({ remaining: Math.max(0, limit - used) }); 
        });
      });
    });
  });
}



app.listen(PORT, () => console.log(`Server running on port ${PORT}`));
