// QueryHub JavaScript Functions

// Initialize when document is ready
$(document).ready(function() {
    // Initialize syntax highlighting
    hljs.highlightAll();
    
    // Auto-dismiss alerts after 5 seconds
    setTimeout(function() {
        $('.alert').alert('close');
    }, 5000);
    
    // Initialize tooltips
    var tooltipTriggerList = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
    var tooltipList = tooltipTriggerList.map(function (tooltipTriggerEl) {
        return new bootstrap.Tooltip(tooltipTriggerEl);
    });
    
    // Debug navigation issues
    console.log('QueryHub JavaScript loaded successfully');
    
    // Ensure navbar toggle works
    $('.navbar-toggler').click(function() {
        console.log('Navbar toggler clicked');
    });
    
    // Log navigation clicks
    $('.nav-link').click(function() {
        console.log('Navigation link clicked:', $(this).attr('href'));
    });
});

// Fix for navigation issues - ensure links work
document.addEventListener('DOMContentLoaded', function() {
    // Force navigation links to work properly
    const navLinks = document.querySelectorAll('.nav-link[asp-controller]');
    navLinks.forEach(link => {
        link.addEventListener('click', function(e) {
            console.log('Nav link clicked:', this.textContent.trim());
        });
    });
});

// Voting functionality
function voteQuestion(questionId, isUpvote) {
    if (!isUserAuthenticated()) {
        showLoginRequired();
        return;
    }
    
    const voteBtn = event.target.closest('.vote-btn');
    const voteCountElement = document.getElementById(`question-votes-${questionId}`);
    
    // Show loading state
    voteBtn.disabled = true;
    voteBtn.innerHTML = '<div class="loading"></div>';
    
    $.ajax({
        url: '/Questions/Vote',
        type: 'POST',
        data: {
            id: questionId,
            isUpvote: isUpvote,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                voteCountElement.textContent = response.votes;
                
                // Update button states
                updateVoteButtons(voteBtn, isUpvote);
                
                // Show success feedback
                showToast('Vote recorded!', 'success');
            } else {
                showToast('Failed to record vote', 'error');
            }
        },
        error: function() {
            showToast('An error occurred', 'error');
        },
        complete: function() {
            // Restore button
            voteBtn.disabled = false;
            voteBtn.innerHTML = isUpvote ? '<i class="fas fa-chevron-up"></i>' : '<i class="fas fa-chevron-down"></i>';
        }
    });
}

// Answer voting
function voteAnswer(answerId, isUpvote) {
    if (!isUserAuthenticated()) {
        showLoginRequired();
        return;
    }
    
    const voteBtn = event.target.closest('.vote-btn');
    const voteCountElement = document.getElementById(`answer-votes-${answerId}`);
    
    voteBtn.disabled = true;
    voteBtn.innerHTML = '<div class="loading"></div>';
    
    $.ajax({
        url: '/Answers/Vote',
        type: 'POST',
        data: {
            id: answerId,
            isUpvote: isUpvote,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                voteCountElement.textContent = response.votes;
                updateVoteButtons(voteBtn, isUpvote);
                showToast('Vote recorded!', 'success');
            } else {
                showToast('Failed to record vote', 'error');
            }
        },
        error: function() {
            showToast('An error occurred', 'error');
        },
        complete: function() {
            voteBtn.disabled = false;
            voteBtn.innerHTML = isUpvote ? '<i class="fas fa-chevron-up"></i>' : '<i class="fas fa-chevron-down"></i>';
        }
    });
}

// Update vote button states
function updateVoteButtons(clickedBtn, isUpvote) {
    const voteSection = clickedBtn.closest('.vote-section');
    const upBtn = voteSection.querySelector('[data-vote="up"]');
    const downBtn = voteSection.querySelector('[data-vote="down"]');
    
    // Reset all buttons
    upBtn.classList.remove('btn-success', 'btn-outline-success');
    downBtn.classList.remove('btn-danger', 'btn-outline-danger');
    
    if (isUpvote) {
        upBtn.classList.add('btn-success');
        downBtn.classList.add('btn-outline-danger');
    } else {
        upBtn.classList.add('btn-outline-success');
        downBtn.classList.add('btn-danger');
    }
}

// Accept answer
function acceptAnswer(answerId) {
    if (!isUserAuthenticated()) {
        showLoginRequired();
        return;
    }
    
    const acceptBtn = event.target.closest('.accept-btn');
    acceptBtn.disabled = true;
    acceptBtn.innerHTML = '<div class="loading"></div>';
    
    $.ajax({
        url: '/Answers/Accept',
        type: 'POST',
        data: {
            id: answerId,
            __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
        },
        success: function(response) {
            if (response.success) {
                // Replace accept button with accepted icon
                acceptBtn.outerHTML = '<div class="mt-2"><i class="fas fa-check-circle fa-2x text-success" title="Accepted Answer"></i></div>';
                
                // Add accepted answer styling to card
                const card = acceptBtn.closest('.card');
                card.classList.add('border-success');
                
                // Add accepted header if not exists
                if (!card.querySelector('.card-header')) {
                    const cardBody = card.querySelector('.card-body');
                    const header = document.createElement('div');
                    header.className = 'card-header bg-success text-white';
                    header.innerHTML = '<i class="fas fa-check-circle me-2"></i>Accepted Answer';
                    card.insertBefore(header, cardBody);
                }
                
                // Show the message from the server
                showToast(response.message || 'Answer accepted!', 'success');
            } else {
                // Show the specific error message from the server
                showToast(response.message || 'Failed to accept answer', 'error');
            }
        },
        error: function() {
            showToast('An error occurred', 'error');
        },
        complete: function() {
            if (acceptBtn.parentNode) {
                acceptBtn.disabled = false;
                acceptBtn.innerHTML = '<i class="fas fa-check"></i>';
            }
        }
    });
}

// Comment functionality
function showCommentForm(type, id) {
    const form = document.getElementById(`comment-form-${type}-${id}`);
    form.style.display = 'block';
    form.querySelector('input').focus();
}

function hideCommentForm(type, id) {
    const form = document.getElementById(`comment-form-${type}-${id}`);
    form.style.display = 'none';
    form.querySelector('input').value = '';
}

function submitComment(type, id, button) {
    if (!isUserAuthenticated()) {
        showLoginRequired();
        return;
    }
    
    const form = button.closest('.comment-form');
    const input = form.querySelector('input');
    const content = input.value.trim();
    
    if (!content) {
        input.focus();
        return;
    }
    
    button.disabled = true;
    button.innerHTML = '<div class="loading"></div>';
    
    const url = type === 'question' ? '/Questions/AddComment' : '/Answers/AddComment';
    const data = {
        content: content,
        __RequestVerificationToken: $('input[name="__RequestVerificationToken"]').val()
    };
    
    if (type === 'question') {
        data.questionId = id;
    } else {
        data.answerId = id;
    }
    
    $.ajax({
        url: url,
        type: 'POST',
        data: data,
        success: function(response) {
            if (response.success) {
                // Add comment to the list
                const commentsSection = document.getElementById(`${type}-comments-${id}`);
                const commentHtml = `
                    <div class="comment border-start border-3 border-light ps-3 py-2">
                        <span class="comment-content">${response.comment.content}</span>
                        <small class="text-muted ms-2">
                            – <strong>${response.comment.userName}</strong> ${response.comment.createdDate}
                        </small>
                    </div>
                `;
                
                // Insert before the comment form
                form.insertAdjacentHTML('beforebegin', commentHtml);
                
                // Clear and hide form
                input.value = '';
                hideCommentForm(type, id);
                
                showToast('Comment added!', 'success');
            } else {
                showToast(response.message || 'Failed to add comment', 'error');
            }
        },
        error: function() {
            showToast('An error occurred', 'error');
        },
        complete: function() {
            button.disabled = false;
            button.innerHTML = '<i class="fas fa-paper-plane"></i>';
        }
    });
}

// Utility functions
function isUserAuthenticated() {
    // Check if user is authenticated (you can modify this based on your auth implementation)
    return document.querySelector('.navbar .dropdown-toggle') !== null;
}

function showLoginRequired() {
    if (confirm('You need to be logged in to perform this action. Would you like to login now?')) {
        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname);
    }
}

function showToast(message, type = 'info') {
    // Create toast element
    const toastHtml = `
        <div class="toast align-items-center text-white bg-${type === 'success' ? 'success' : type === 'error' ? 'danger' : 'primary'} border-0" role="alert">
            <div class="d-flex">
                <div class="toast-body">
                    <i class="fas fa-${type === 'success' ? 'check-circle' : type === 'error' ? 'exclamation-triangle' : 'info-circle'} me-2"></i>
                    ${message}
                </div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        </div>
    `;
    
    // Add to toast container (create if doesn't exist)
    let toastContainer = document.querySelector('.toast-container');
    if (!toastContainer) {
        toastContainer = document.createElement('div');
        toastContainer.className = 'toast-container position-fixed bottom-0 end-0 p-3';
        document.body.appendChild(toastContainer);
    }
    
    toastContainer.insertAdjacentHTML('beforeend', toastHtml);
    
    // Show toast
    const toastElement = toastContainer.lastElementChild;
    const toast = new bootstrap.Toast(toastElement, { delay: 3000 });
    toast.show();
    
    // Remove element after hiding
    toastElement.addEventListener('hidden.bs.toast', function() {
        toastElement.remove();
    });
}

// Search enhancements
function initializeSearch() {
    const searchInput = document.querySelector('input[name="search"]');
    if (searchInput) {
        // Add search suggestions (you can expand this)
        searchInput.addEventListener('input', function() {
            // Implement search suggestions if needed
        });
    }
}

// Form enhancements
function enhanceForms() {
    // Auto-resize textareas
    document.querySelectorAll('textarea').forEach(textarea => {
        textarea.addEventListener('input', function() {
            this.style.height = 'auto';
            this.style.height = (this.scrollHeight) + 'px';
        });
    });
    
    // Character counter for comment inputs
    document.querySelectorAll('input[maxlength]').forEach(input => {
        const maxLength = input.getAttribute('maxlength');
        if (maxLength) {
            const counter = document.createElement('small');
            counter.className = 'text-muted ms-2';
            input.parentNode.appendChild(counter);
            
            function updateCounter() {
                const remaining = maxLength - input.value.length;
                counter.textContent = `${remaining} characters remaining`;
                counter.className = remaining < 50 ? 'text-warning ms-2' : 'text-muted ms-2';
            }
            
            input.addEventListener('input', updateCounter);
            updateCounter();
        }
    });
}

// Initialize enhancements
document.addEventListener('DOMContentLoaded', function() {
    initializeSearch();
    enhanceForms();
    
    // Add click handlers for vote buttons
    document.addEventListener('click', function(e) {
        if (e.target.closest('.vote-btn')) {
            const btn = e.target.closest('.vote-btn');
            const isUpvote = btn.getAttribute('data-vote') === 'up';
            const type = btn.getAttribute('data-type');
            const section = btn.closest('.vote-section');
            
            if (type === 'answer') {
                const answerId = section.getAttribute('data-answer-id');
                voteAnswer(answerId, isUpvote);
            } else {
                const questionId = section.getAttribute('data-question-id');
                voteQuestion(questionId, isUpvote);
            }
        }
        
        if (e.target.closest('.accept-btn')) {
            const btn = e.target.closest('.accept-btn');
            const answerId = btn.getAttribute('data-answer-id');
            acceptAnswer(answerId);
        }
    });
    
    // Handle Enter key in comment inputs
    document.addEventListener('keypress', function(e) {
        if (e.key === 'Enter' && e.target.closest('.comment-form input')) {
            const submitBtn = e.target.closest('.comment-form').querySelector('button[onclick*="submitComment"]');
            if (submitBtn) {
                submitBtn.click();
            }
        }
    });

    // Share button functionality
    document.addEventListener('click', function(e) {
        const shareBtn = e.target.closest('.share-btn');
        if (shareBtn) {
            e.preventDefault();
            
            const type = shareBtn.getAttribute('data-type');
            const id = shareBtn.getAttribute('data-id');
            let url = window.location.origin;
            
            if (type === 'question') {
                // For questions, create a direct URL to the question details page
                url += `/Questions/Details/${id}`;
            } else if (type === 'answer') {
                // For answers, use current page URL and add anchor to specific answer
                url = window.location.href.split('#')[0].replace(/#.*$/, '');
                url += `#answer-${id}`;
            }
            
            // Try to copy to clipboard
            if (navigator.clipboard && navigator.clipboard.writeText) {
                navigator.clipboard.writeText(url).then(function() {
                    // Show success feedback
                    showShareFeedback(shareBtn, 'Link copied!', 'success');
                }).catch(function(err) {
                    console.error('Failed to copy link: ', err);
                    // Fallback to manual copy
                    showManualCopyPrompt(url);
                });
            } else {
                // Fallback for browsers that don't support clipboard API
                showManualCopyPrompt(url);
            }
        }
    });
});

// Helper function to show share feedback
function showShareFeedback(shareBtn, message, type) {
    const msg = shareBtn.parentElement.querySelector('.share-msg');
    if (msg) {
        msg.textContent = message;
        msg.className = `share-msg text-${type === 'success' ? 'success' : 'warning'} ms-2`;
        msg.style.display = 'inline';
        
        // Hide the message after 3 seconds
        setTimeout(() => {
            msg.style.display = 'none';
        }, 3000);
    }
}

// Helper function to show manual copy prompt
function showManualCopyPrompt(url) {
    // Create a temporary input element
    const tempInput = document.createElement('input');
    tempInput.value = url;
    document.body.appendChild(tempInput);
    
    try {
        tempInput.select();
        tempInput.setSelectionRange(0, 99999); // For mobile devices
        
        // Try to copy using the deprecated execCommand as fallback
        const successful = document.execCommand('copy');
        document.body.removeChild(tempInput);
        
        if (successful) {
            showToast('Link copied to clipboard!', 'success');
        } else {
            // Show prompt dialog if all else fails
            window.prompt('Copy this link to your clipboard: Ctrl+C (Cmd+C on Mac), then press Enter', url);
        }
    } catch (err) {
        document.body.removeChild(tempInput);
        // Show prompt dialog as final fallback
        window.prompt('Copy this link to your clipboard: Ctrl+C (Cmd+C on Mac), then press Enter', url);
    }
}
