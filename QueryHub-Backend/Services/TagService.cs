using QueryHub_Backend.Interfaces;
using QueryHub_Backend.Models;

namespace QueryHub_Backend.Services
{
    public class TagService : ITagService
    {
        private readonly ITagRepository _tagRepository;

        public TagService(ITagRepository tagRepository)
        {
            _tagRepository = tagRepository;
        }

        public async Task<Tag?> GetByIdAsync(int id)
        {
            return await _tagRepository.GetByIdAsync(id);
        }

        public async Task<Tag?> GetByNameAsync(string name)
        {
            return await _tagRepository.GetByNameAsync(name);
        }

        public async Task<IEnumerable<Tag>> GetAllAsync()
        {
            return await _tagRepository.GetAllAsync();
        }

        public async Task<IEnumerable<Tag>> GetByQuestionIdAsync(int questionId)
        {
            return await _tagRepository.GetByQuestionIdAsync(questionId);
        }

        public async Task<IEnumerable<Tag>> SearchAsync(string searchTerm)
        {
            return await _tagRepository.SearchAsync(searchTerm);
        }

        public async Task<Tag> CreateAsync(Tag tag)
        {
            // Check if tag with this name already exists
            var existingTag = await _tagRepository.GetByNameAsync(tag.Name);
            if (existingTag != null)
            {
                throw new InvalidOperationException($"Tag with name '{tag.Name}' already exists");
            }

            tag.CreatedAt = DateTime.UtcNow;
            return await _tagRepository.CreateAsync(tag);
        }

        public async Task<Tag> UpdateAsync(Tag tag)
        {
            var existingTag = await _tagRepository.GetByIdAsync(tag.Id);
            if (existingTag == null)
            {
                throw new ArgumentException("Tag not found");
            }

            // Check if another tag with this name already exists
            var tagWithSameName = await _tagRepository.GetByNameAsync(tag.Name);
            if (tagWithSameName != null && tagWithSameName.Id != tag.Id)
            {
                throw new InvalidOperationException($"Another tag with name '{tag.Name}' already exists");
            }

            return await _tagRepository.UpdateAsync(tag);
        }

        public async Task DeleteAsync(int id)
        {
            var tag = await _tagRepository.GetByIdAsync(id);
            if (tag == null)
            {
                throw new ArgumentException("Tag not found");
            }

            // Check if tag is being used by any questions
            var questionCount = await _tagRepository.GetQuestionCountByTagAsync(id);
            if (questionCount > 0)
            {
                throw new InvalidOperationException($"Cannot delete tag '{tag.Name}' as it is being used by {questionCount} question(s)");
            }

            await _tagRepository.DeleteAsync(id);
        }

        public async Task AddTagToQuestionAsync(int questionId, int tagId)
        {
            await _tagRepository.AddTagToQuestionAsync(questionId, tagId);
        }

        public async Task RemoveTagFromQuestionAsync(int questionId, int tagId)
        {
            await _tagRepository.RemoveTagFromQuestionAsync(questionId, tagId);
        }

        public async Task<int> GetQuestionCountByTagAsync(int tagId)
        {
            return await _tagRepository.GetQuestionCountByTagAsync(tagId);
        }

        public async Task<IEnumerable<Tag>> GetPopularTagsAsync(int limit = 10)
        {
            var allTags = await _tagRepository.GetAllAsync();
            var tagsWithCounts = new List<(Tag Tag, int Count)>();

            foreach (var tag in allTags)
            {
                var count = await _tagRepository.GetQuestionCountByTagAsync(tag.Id);
                tagsWithCounts.Add((tag, count));
            }

            return tagsWithCounts
                .OrderByDescending(t => t.Count)
                .Take(limit)
                .Select(t => t.Tag);
        }
    }
}
