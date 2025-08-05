        public async Task<IActionResult> Profile()
        {
            if (!User.Identity!.IsAuthenticated)
            {
                return RedirectToAction("Login");
            }

            var currentUsername = User.Identity.Name ?? "Demo User";
            
            // Create user object with basic info
            var user = new User
            {
                Id = 1,
                Name = currentUsername,
                Email = User.FindFirst(ClaimTypes.Email)?.Value ?? "demo@queryhub.com",
                Department = User.FindFirst("Department")?.Value ?? "IT Department",
                Avatar = null,
                Reputation = 100,
                JoinedDate = DateTime.Now.AddMonths(-6)
            };

            // Initialize with safe default values
            ViewBag.QuestionsAsked = 0;
            ViewBag.AnswersGiven = 0;
            ViewBag.TotalVotes = 0;
            ViewBag.TotalViews = 0;
            ViewBag.RecentActivity = new List<object>();

            try
            {
                // Get user's questions
                var allQuestions = await _apiService.GetQuestionsAsync("", "", 1, 1000);
                var userQuestions = allQuestions.Where(q => q.UserName.Equals(currentUsername, StringComparison.OrdinalIgnoreCase)).ToList();
                
                // Calculate basic stats
                ViewBag.QuestionsAsked = userQuestions.Count;
                ViewBag.TotalVotes = userQuestions.Sum(q => q.Votes);
                ViewBag.TotalViews = userQuestions.Sum(q => q.Views);
                user.Reputation = Math.Max(100, ViewBag.TotalVotes * 10 + ViewBag.QuestionsAsked * 5);

                // Calculate answers given (simple count for now)
                ViewBag.AnswersGiven = userQuestions.Sum(q => q.AnswerCount);

                // Simple recent activity - just show user's recent questions
                var recentActivity = userQuestions
                    .OrderByDescending(q => q.CreatedDate)
                    .Take(5)
                    .Select(q => new {
                        Type = "question",
                        Title = q.Title,
                        CreatedDate = q.CreatedDate,
                        Id = q.Id,
                        QuestionId = q.Id
                    })
                    .ToList();

                ViewBag.RecentActivity = recentActivity;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading profile data for user: {Username}", currentUsername);
                // Keep default values
            }

            return View(user);
        }
