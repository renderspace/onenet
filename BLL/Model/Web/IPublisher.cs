namespace One.Net.BLL
{
    public interface IPublisher
    {

        /// <summary>
        /// Publishes the content by copying the preview version to publish version (overwriting, if it exists)
        /// </summary>
        /// <param name="frequentQuestionId"></param>
        bool Publish(int id);

        /// <summary>
        /// Deletes the published version of object, if it exists. In effect, unpublishing it.
        /// </summary>
        /// <param name="frequentQuestionId"></param>
        bool UnPublish(int id);

        /// <summary>
        /// Marks the object for deletion .... marks mark_for_deletion flag for offline copy for deletion....
        /// </summary>
        /// <param name="frequentQuestionId"></param>
        bool MarkForDeletion(int id);

        /// <summary>
        /// Copies the published FAQ to preview FAQ, overwriting it if it exists.
        /// If FAQ way previously marked for deletion, it's not marked for deletion after the call to this method.
        /// If FAQ had any pending changes in preview, they are discarded and reverted to published state.
        /// Also works as undelete if object was never published.
        /// </summary>
        /// <param name="frequentQuestionId"></param>
        bool RevertToPublished(int id);
    }
}
