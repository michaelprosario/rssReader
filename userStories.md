- draft the following rss reader app using blazor server



## AppCore rules

- encapsulate business logic and data management into service clainsses
- service classes should depend on repository interfaces or providers

## App Infrastructure

- repositories or providers should be implemented in the AppInfrastructure project
- Data repositories should depend on Entity Framework Core and Sqllite


RSS Reader App User Stories




## Article Preview
As a user, I want to expand article previews directly in the feed view so that I can read short articles or evaluate longer articles without navigating away from my main feed.
As a user, I want to see enough of the article content in the preview to determine if I want to read the full article so that I can efficiently triage my reading queue.
As a user, I want preview actions (bookmark, mark as read, open full article) to be easily accessible so that I can take action on articles directly from the preview without additional navigation.

## Core Feed Management

# Add/Remove Feeds
As a user, I want to add RSS feeds by entering a blog URL so that the app can automatically discover and subscribe to available feeds without me needing to find the exact RSS link.

- As a user, I want to add feeds by pasting direct RSS, Atom, or JSON feed URLs so that I can subscribe to feeds regardless of their format.
- As a user, I want to remove feeds from my subscription list so that I can declutter my reading experience and stop receiving updates from sources I'm no longer interested in.
- As a user, I want to see a confirmation dialog when removing feeds so that I don't accidentally delete feeds I want to keep.

## Side Navigation
As a user, I want quick access to all my feeds, bookmarks, and tags through side navigation so that I can efficiently move between different sections of the app.
As a user, I want to see unread counts next to each feed in the navigation so that I can prioritize which feeds to check first.
As a user, I want to collapse and expand the side navigation so that I can maximize reading space when focusing on content.


## Bulk Operations
As a user, I want to import an OPML file containing my existing feed subscriptions so that I can quickly migrate from another RSS reader without manually adding each feed.
As a user, I want to export my current feed subscriptions as an OPML file so that I can back up my subscriptions or migrate to another RSS reader.
As a user, I want to see a progress indicator during import/export operations so that I know the process is working and can estimate completion time.

## Feed Refresh Settings
As a user, I want to set a global refresh interval for all feeds so that I can control how often the app checks for new content without configuring each feed individually.
As a user, I want to set custom refresh intervals for individual feeds so that I can check frequently-updated feeds more often and slow-updating feeds less often.
As a user, I want to manually refresh feeds on demand so that I can immediately check for new content when I want to read.


## Reading Experience

##  Unified Feed View
As a user, I want to see all new posts from all my subscribed feeds in one chronological stream so that I can efficiently catch up on all my reading in one place.
As a user, I want to see which feed each post comes from in the unified view so that I can quickly identify the source without having to open the individual post.
Individual Feed Views
As a user, I want to view posts from a specific feed so that I can focus on content from a particular source when I'm interested in updates from that specific blog or publication.
As a user, I want to see the feed's metadata (title, description, last updated) so that I can understand more about the source and its posting frequency.


## Article Display
As a user, I want to read articles in a clean, formatted view that preserves the original styling so that I can see the content as the author intended.
As a user, I want to switch to a simplified reader mode so that I can focus on the content without distractions when the original formatting is cluttered or hard to read.
As a user, I want the article view to be responsive and readable on different screen sizes so that I can comfortably read on desktop, tablet, or mobile devices.

## Full-Text Content
As a user, I want the app to automatically fetch complete article content when feeds only provide summaries so that I can read full articles without having to visit the original website.
As a user, I want to see an indicator when full content has been fetched so that I know I'm reading the complete article rather than just a summary.

# Mark as Read/Unread
As a user, I want articles to be automatically marked as read when I scroll past them or spend time viewing them so that I can track what content I've already seen without manual action.
As a user, I want to manually mark articles as read or unread so that I can control my reading status for articles I want to revisit or skip.
As a user, I want to mark all articles in a feed as read so that I can quickly clear out feeds with many accumulated posts.

# Search Functionality
As a user, I want to search across all my posts by title and content so that I can quickly find specific articles I remember reading or want to reference.
As a user, I want to filter search results by specific feeds so that I can narrow down my search when I remember which source published the content I'm looking for.
As a user, I want to see search results with highlighted matching text so that I can quickly identify why each result matched my search query.

## Bookmarking System


## Save for Later
As a user, I want to bookmark articles with a single click so that I can quickly save interesting content without interrupting my reading flow.
As a user, I want to see a visual indicator when an article is bookmarked so that I can easily identify which articles I've already saved.
As a user, I want to access all my bookmarked articles in a dedicated section so that I can easily find and review content I've saved for later.

## Dashboard
As a user, I want to see a dashboard with unread counts for each feed so that I can quickly identify which sources have new content waiting.
As a user, I want to see my recent bookmarks on the dashboard so that I can quickly access articles I've recently saved without navigating to the bookmarks section.
As a user, I want to see my most-used tags on the dashboard so that I can quickly access my most important content categories.


## Read Status
As a user, I want to track which bookmarked articles I've actually read so that I can distinguish between articles I've saved and articles I've both saved and consumed.
As a user, I want to filter my bookmarks by read/unread status so that I can focus on either reviewing content I've already read or tackling my unread saved articles.

## Export Options
As a user, I want to export my bookmarked articles as markdown files so that I can create backups or use the content in other applications that support markdown format.
As a user, I want to choose which bookmark metadata to include in exports (tags, dates, read status) so that I can customize the exported content to my needs.

## Tagging System

## Custom Tags
As a user, I want to create custom tags and assign them to bookmarked articles so that I can organize my saved content by topics, projects, or any categorization system that makes sense to me.
As a user, I want to assign multiple tags to a single bookmarked article so that I can categorize content that spans multiple topics or projects.
As a user, I want to see tag suggestions as I type so that I can reuse existing tags and maintain consistency in my tagging system.

## Tag Management
As a user, I want to edit tag names so that I can improve my tagging system as it evolves and correct typos in tag names.
As a user, I want to merge tags so that I can consolidate tags that represent the same concept but were created with different names.
As a user, I want to delete unused tags so that I can keep my tag list clean and focused on categories I actually use.
As a user, I want to see how many articles are associated with each tag so that I can understand the size and importance of different categories in my bookmark collection.

## Tag-based Filtering
As a user, I want to filter my bookmarks by a single tag so that I can view all articles related to a specific topic or project.
As a user, I want to filter my bookmarks by multiple tags simultaneously so that I can find articles that exist at the intersection of multiple topics or categories.
As a user, I want to exclude certain tags from my filter so that I can find articles that match some criteria while avoiding others.

## Additional Features

## Keyboard Shortcuts
As a user, I want to use keyboard shortcuts to navigate between articles so that I can efficiently browse through my feeds without using the mouse.
As a user, I want to use keyboard shortcuts to mark articles as read/unread, bookmark them, and perform other common actions so that I can manage my reading workflow efficiently.
As a user, I want to see a list of available keyboard shortcuts so that I can learn and remember the shortcuts that will speed up my workflow.
User Interface Elements

