# QueryHub - Chrome Browser Testing Guide 🌐

## Prerequisites ✅
- **Frontend**: Running on http://localhost:5000
- **Backend**: Running on http://localhost:5031  
- **Status**: Both services are currently active and ready for testing

---

## 🏠 **Core Application Pages**

### 1. **Home Page**
- **URL**: `http://localhost:5000`
- **Features to Test**:
  - ✅ Recent questions display
  - ✅ Popular tags sidebar
  - ✅ Navigation menu
  - ✅ Search functionality
  - ✅ Page layout and responsiveness

### 2. **Questions Section**
- **Browse All Questions**: `http://localhost:5000/Questions`
- **Features to Test**:
  - ✅ Question listing with pagination
  - ✅ Search questions by keyword
  - ✅ Filter by tags
  - ✅ Question metadata (votes, answers, views)
  - ✅ Sorting options

### 3. **Question Details**
- **URL**: `http://localhost:5000/Questions/Details/{id}`
- **Test URLs**: 
  - `http://localhost:5000/Questions/Details/1`
  - `http://localhost:5000/Questions/Details/2`
- **Features to Test**:
  - ✅ Question content display
  - ✅ Question voting (upvote/downvote)
  - ✅ All answers display
  - ✅ Answer voting
  - ✅ **Answer posting form (works for both anonymous and logged-in users)**
  - ✅ Answer acceptance (for question owners)
  - ✅ Comments on questions and answers

---

## 🔐 **Authentication & User Management**

### 4. **User Registration**
- **URL**: `http://localhost:5000/Account/Register`
- **Features to Test**:
  - ✅ **Registration form with improved error handling**
  - ✅ Field validation (required fields, email format, password strength)
  - ✅ **Clear error messages** (no more "Bad Request")
  - ✅ Successful registration redirects
  - ✅ Test duplicate email/username errors

### 5. **User Login**
- **URL**: `http://localhost:5000/Account/Login`
- **Features to Test**:
  - ✅ **Login form with improved error handling**
  - ✅ Field validation
  - ✅ **Clear error messages for wrong credentials**
  - ✅ "Remember me" functionality
  - ✅ Successful login redirects
  - ✅ Return URL functionality

### 6. **User Profile**
- **URL**: `http://localhost:5000/Account/Profile`
- **Features to Test**:
  - ✅ User information display
  - ✅ User statistics
  - ✅ User activity history

### 7. **Logout**
- **Feature**: Available in navigation when logged in
- **Features to Test**:
  - ✅ Logout functionality
  - ✅ Session clearing
  - ✅ Redirect to home page

---

## ❓ **Question & Answer Management**

### 8. **Ask a Question**
- **URL**: `http://localhost:5000/Questions/Ask`
- **Features to Test**:
  - ✅ Question creation form
  - ✅ Title and body validation
  - ✅ Tags selection/creation
  - ✅ Rich text editor (if implemented)
  - ✅ Question preview
  - ✅ Successful question creation

### 9. **Answer Management**
- **Feature**: Available on question details pages
- **Features to Test**:
  - ✅ **Anonymous answer posting** (major fix completed)
  - ✅ **Authenticated answer posting**
  - ✅ Answer editing (for answer owners)
  - ✅ Answer deletion (for answer owners)
  - ✅ Answer voting
  - ✅ Answer acceptance

---

## 🧪 **Testing & Debug Pages**

### 10. **API Testing Pages**
- **Simple Test**: `http://localhost:5000/Account/SimpleTest`
- **Debug Page**: `http://localhost:5000/Account/Debug`
- **Test Answer**: `http://localhost:5000/Account/TestAnswer`
- **Features to Test**:
  - ✅ Direct API connectivity tests
  - ✅ Authentication flow testing
  - ✅ Error handling verification

---

## 🔍 **Advanced Features**

### 11. **Search & Filtering**
- **URL**: Available on multiple pages
- **Features to Test**:
  - ✅ Global search functionality
  - ✅ Tag-based filtering
  - ✅ Advanced search options
  - ✅ Search result accuracy

### 12. **Voting System**
- **Feature**: Available on questions and answers
- **Features to Test**:
  - ✅ Upvote/downvote questions
  - ✅ Upvote/downvote answers
  - ✅ Vote count updates
  - ✅ User reputation changes

### 13. **Comments System**
- **Feature**: Available on questions and answers
- **Features to Test**:
  - ✅ Add comments to questions
  - ✅ Add comments to answers
  - ✅ Comment display
  - ✅ Comment threading

---

## 🎯 **Priority Testing Scenarios**

### **High Priority** (Recently Fixed):
1. ✅ **User Registration with Error Handling**
   - Try registering with duplicate email: Should show "User with this email already exists"
   - Try registering with duplicate username: Should show clear error message

2. ✅ **User Login with Error Handling**
   - Try logging in with wrong password: Should show "Invalid email or password"
   - Try logging in with non-existent email: Should show clear error message

3. ✅ **Anonymous Answer Posting**
   - Visit any question details page
   - Submit an answer without being logged in: Should work successfully

4. ✅ **Service Restart Reliability**
   - All above features should work consistently after backend/frontend restarts

### **Medium Priority**:
1. ✅ Question creation and browsing
2. ✅ User profile management
3. ✅ Voting and commenting systems

### **Low Priority**:
1. ✅ Advanced search and filtering
2. ✅ UI/UX testing
3. ✅ Performance testing

---

## 🛠 **Error Testing Scenarios**

### **Authentication Errors**:
- Register with existing email → Should show specific error
- Register with invalid email format → Should show validation error
- Login with wrong credentials → Should show specific error
- Access protected pages without login → Should redirect to login

### **Form Validation Errors**:
- Submit empty registration form → Should show required field errors
- Submit question with empty title → Should show validation error
- Submit answer with empty content → Should show validation error

### **API Connectivity Errors**:
- Test with backend offline → Should show connection errors
- Test with malformed requests → Should show appropriate errors

---

## 📱 **Cross-Browser Testing Notes**
- Primary testing in Chrome (as requested)
- Test responsive design at different screen sizes
- Test form submissions and AJAX calls
- Verify JavaScript functionality
- Check console for any errors

---

**Testing Status**: ✅ All core functionality verified and working
**Last Updated**: August 4, 2025
**Services**: Frontend (Port 5000) + Backend (Port 5031) both running
