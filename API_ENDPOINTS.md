# QueryHub Backend API Endpoints

## Base URL: `http://localhost:5031`

## 📋 **Complete API Endpoints List**

### 🔐 **Authentication Endpoints** (`/api/auth`)
- **POST** `/api/auth/register` - Register new user
- **POST** `/api/auth/login` - Login user  
- **POST** `/api/auth/validate-token` - Validate JWT token

### ❓ **Questions Endpoints** (`/api/questions`)
- **GET** `/api/questions` - Get all questions (supports ?search= and ?tag= parameters)
- **GET** `/api/questions/{id}` - Get question by ID
- **POST** `/api/questions` - Create new question (✅ **NO AUTH REQUIRED**)
- **PUT** `/api/questions/{id}` - Update question (requires auth)
- **DELETE** `/api/questions/{id}` - Delete question (requires auth)

### 💬 **Answers Endpoints** (`/api/answers`)
- **GET** `/api/answers/{id}` - Get answer by ID
- **GET** `/api/answers/question/{questionId}` - Get answers for specific question
- **GET** `/api/answers/user/{userId}` - Get answers by user
- **POST** `/api/answers` - Create new answer (✅ **NO AUTH REQUIRED**)
- **PUT** `/api/answers/{id}` - Update answer (requires auth)
- **DELETE** `/api/answers/{id}` - Delete answer (requires auth)

### 🏷️ **Tags Endpoints** (`/api/tags`)
- **GET** `/api/tags` - Get all tags

### 🗨️ **Comments Endpoints** (`/api/comments`)
- **GET** `/api/comments/question/{questionId}` - Get comments for question
- **GET** `/api/comments/answer/{answerId}` - Get comments for answer  
- **POST** `/api/comments` - Create new comment (requires auth)
- **PUT** `/api/comments/{id}` - Update comment (requires auth)
- **DELETE** `/api/comments/{id}` - Delete comment (requires auth)

### 👍 **Votes Endpoints** (`/api/votes`)
- **POST** `/api/votes` - Create/update vote (requires auth)
- **DELETE** `/api/votes/{id}` - Delete vote (requires auth)

---

## 🎯 **Answer Question Endpoint (What You Need)**

### **Create Answer:**
```http
POST /api/answers
Content-Type: application/json

{
    "Body": "Your answer content here (minimum 10 characters)",
    "QuestionId": 1
}
```

**Response (201 Created):**
```json
{
    "id": 5,
    "body": "Your answer content here",
    "questionId": 1,
    "userId": 1,
    "votes": 0,
    "isAccepted": false,
    "createdAt": "2025-08-04T...",
    "updatedAt": "2025-08-04T..."
}
```

---

## 📖 **Swagger Documentation**
Access full interactive API documentation at:
**http://localhost:5031**

---

## ✅ **Key Features**
- **No Authentication Required**: Questions and Answers can be posted anonymously
- **Authentication Optional**: Login/Register available for personalized features
- **CORS Enabled**: Frontend can call backend APIs
- **JSON Format**: All requests/responses use JSON
- **Error Handling**: Proper HTTP status codes and error messages
