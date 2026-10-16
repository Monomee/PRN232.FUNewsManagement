-- =============================================
-- Database: FUNewsManagement Schema & Seed Data (v2)
-- Distributed and Intelligent News Management System
-- =============================================
USE master;
GO
IF NOT EXISTS (SELECT name FROM sys.databases WHERE name = 'FUNewsManagement')
BEGIN
    CREATE DATABASE FUNewsManagement;
END;
GO
USE FUNewsManagement;
GO

IF OBJECT_ID('dbo.RefreshToken', 'U') IS NOT NULL DROP TABLE dbo.RefreshToken;
IF OBJECT_ID('dbo.AuditLog', 'U') IS NOT NULL DROP TABLE dbo.AuditLog;
IF OBJECT_ID('dbo.NewsTag', 'U') IS NOT NULL DROP TABLE dbo.NewsTag;
IF OBJECT_ID('dbo.NewsArticle', 'U') IS NOT NULL DROP TABLE dbo.NewsArticle;
IF OBJECT_ID('dbo.Tag', 'U') IS NOT NULL DROP TABLE dbo.Tag;
IF OBJECT_ID('dbo.Category', 'U') IS NOT NULL DROP TABLE dbo.Category;
IF OBJECT_ID('dbo.SystemAccount', 'U') IS NOT NULL DROP TABLE dbo.SystemAccount;
GO

CREATE TABLE dbo.SystemAccount (
    AccountID smallint NOT NULL PRIMARY KEY,
    AccountName nvarchar(100) NULL,
    AccountEmail nvarchar(70) NULL,
    AccountRole int NULL,
    AccountPassword nvarchar(70) NULL
);
GO

CREATE TABLE dbo.Category (
    CategoryID smallint NOT NULL PRIMARY KEY,
    CategoryName nvarchar(100) NOT NULL,
    CategoryDesciption nvarchar(250) NOT NULL,
    ParentCategoryID smallint NULL,
    IsActive bit NULL,
    CONSTRAINT FK_Category_Category FOREIGN KEY (ParentCategoryID) REFERENCES dbo.Category(CategoryID)
);
GO

CREATE TABLE dbo.Tag (
    TagID int NOT NULL PRIMARY KEY,
    TagName nvarchar(50) NULL,
    Note nvarchar(400) NULL
);
GO

CREATE TABLE dbo.NewsArticle (
    NewsArticleID nvarchar(20) NOT NULL PRIMARY KEY,
    NewsTitle nvarchar(400) NULL,
    Headline nvarchar(150) NOT NULL,
    CreatedDate datetime NULL,
    NewsContent nvarchar(4000) NULL,
    NewsSource nvarchar(400) NULL,
    CategoryID smallint NULL,
    NewsStatus bit NULL,
    CreatedByID smallint NULL,
    UpdatedByID smallint NULL,
    ModifiedDate datetime NULL,
    ImageUrl nvarchar(500) NULL,
    ViewCount int NOT NULL DEFAULT 0,
    CONSTRAINT FK_NewsArticle_Category FOREIGN KEY (CategoryID) REFERENCES dbo.Category(CategoryID),
    CONSTRAINT FK_NewsArticle_SystemAccount FOREIGN KEY (CreatedByID) REFERENCES dbo.SystemAccount(AccountID)
);
GO

CREATE TABLE dbo.NewsTag (
    NewsArticleID nvarchar(20) NOT NULL,
    TagID int NOT NULL,
    CONSTRAINT PK_NewsTag PRIMARY KEY (NewsArticleID, TagID),
    CONSTRAINT FK_NewsTag_NewsArticle FOREIGN KEY (NewsArticleID) REFERENCES dbo.NewsArticle(NewsArticleID) ON DELETE CASCADE,
    CONSTRAINT FK_NewsTag_Tag FOREIGN KEY (TagID) REFERENCES dbo.Tag(TagID)
);
GO

CREATE TABLE dbo.RefreshToken (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    Token nvarchar(200) NOT NULL,
    JwtId nvarchar(100) NOT NULL,
    IsUsed bit NOT NULL DEFAULT 0,
    IsRevoked bit NOT NULL DEFAULT 0,
    ExpiryDate datetime NOT NULL,
    AccountId smallint NOT NULL,
    CONSTRAINT FK_RefreshToken_SystemAccount FOREIGN KEY (AccountId) REFERENCES dbo.SystemAccount(AccountID) ON DELETE CASCADE
);
GO

CREATE TABLE dbo.AuditLog (
    Id int IDENTITY(1,1) NOT NULL PRIMARY KEY,
    UserId nvarchar(100) NULL,
    Action nvarchar(50) NOT NULL,
    EntityName nvarchar(100) NOT NULL,
    Timestamp datetime NOT NULL DEFAULT GETDATE(),
    BeforeJson nvarchar(max) NULL,
    AfterJson nvarchar(max) NULL
);
GO

-- =============================================
-- SEED DATA: SystemAccount
-- =============================================
INSERT INTO dbo.SystemAccount (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES (0, N'Administrator', N'admin@FUNewsManagementSystem.org', 0, N'@@abc123@@');
INSERT INTO dbo.SystemAccount (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES (1, N'Emma William', N'EmmaWilliam@FUNewsManagement.org', 2, N'@1');
INSERT INTO dbo.SystemAccount (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES (2, N'Olivia James', N'OliviaJames@FUNewsManagement.org', 2, N'@1');
INSERT INTO dbo.SystemAccount (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES (3, N'Isabella David', N'IsabellaDavid@FUNewsManagement.org', 1, N'@1');
INSERT INTO dbo.SystemAccount (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES (4, N'Michael Charlotte', N'MichaelCharlotte@FUNewsManagement.org', 1, N'@1');
INSERT INTO dbo.SystemAccount (AccountID, AccountName, AccountEmail, AccountRole, AccountPassword) VALUES (5, N'Steve Paris', N'SteveParis@FUNewsManagement.org', 1, N'@1');
GO

-- =============================================
-- SEED DATA: Category
-- =============================================
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (1, N'Academic news', N'This category can include articles about research findings, faculty appointments and promotions, and other academic-related announcements.', NULL, 1);
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (2, N'Student Affairs', N'This category can include articles about student activities, events, and initiatives, such as student clubs, organizations and sports.', NULL, 1);
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (3, N'Campus Safety', N'This category can include articles about incidents and safety measures implemented on campus to ensure the safety of students and faculty.', NULL, 1);
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (4, N'Alumni News', N'This category can include articles about the achievements and accomplishments of former students and alumni, such as graduations, job promotions and career successes.', NULL, 1);
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (5, N'Capstone Project News', N'This category is typically a comprehensive and detailed report created as part of an academic or professional capstone project.', NULL, 0);
-- Sub-categories under Academic news (ParentCategoryID = 1)
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (6, N'Software Engineering', N'Software design patterns, agile processes, and cloud computing curriculum.', 1, 1);
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (7, N'Artificial Intelligence', N'Machine learning algorithms, robotics, and generative AI research.', 1, 1);
-- Sub-category under Student Affairs (ParentCategoryID = 2)
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (8, N'Student Clubs & Sports', N'Extracurricular clubs, athletic teams, tournaments, and student life.', 2, 1);
-- Sub-category under Alumni News (ParentCategoryID = 4)
INSERT INTO dbo.Category (CategoryID, CategoryName, CategoryDesciption, ParentCategoryID, IsActive) VALUES (9, N'Career & Mentorship', N'Alumni networking, career fairs, resume reviews, and mentoring programs.', 4, 1);
GO

-- =============================================
-- SEED DATA: Tag
-- =============================================
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (1, N'Education', N'Education Note');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (2, N'Technology', N'Technology Note');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (3, N'Research', N'Research Note');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (4, N'Innovation', N'Innovation Note');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (5, N'Campus Life', N'Campus Life Note');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (6, N'Faculty', N'Faculty Achievements');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (7, N'Alumni', N'Alumni News');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (8, N'Events', N'University Events');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (9, N'Resources', N'Campus Resources');
INSERT INTO dbo.Tag (TagID, TagName, Note) VALUES (10, N'Campus Security', N'Campus safety and emergency guidelines');
GO

-- =============================================
-- SEED DATA: NewsArticle
-- =============================================
INSERT INTO dbo.NewsArticle (NewsArticleID, NewsTitle, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID, UpdatedByID, ModifiedDate, ImageUrl, ViewCount) VALUES (N'1', N'University FU Celebrates Success of Alumni in Various Fields', N'University FU Celebrates Success of Alumni in Various Fields', '2024-05-05 00:00:00', N'University FU recently commemorated the achievements of its esteemed alumni who have excelled in a multitude of fields, showcasing the impact of the institution''s education on their professional journeys. Diverse Success Stories: From successful entrepreneurs to renowned artists, University X''s alumni have made significant strides in various industries, reflecting the versatility of the education provided. Networking Opportunities: The university''s strong alumni network played a crucial role in fostering connections and collaborations among graduates, contributing to their success. Alumni Contributions: Many alumni have also given back to the university through mentorship programs, scholarships, and guest lectures, enriching the current student experience.', N'N/A', 4, 1, 3, 3, '2024-05-05 00:00:00', 'https://images.unsplash.com/photo-1523050854058-8df90110c9f1?w=800', 125);
INSERT INTO dbo.NewsArticle (NewsArticleID, NewsTitle, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID, UpdatedByID, ModifiedDate, ImageUrl, ViewCount) VALUES (N'2', N'Alumni Association Launches Mentorship Program for Recent Graduates', N'Alumni Association Launches Mentorship Program for Recent Graduates', '2024-05-05 00:00:00', N'The Alumni Association of University FU recently unveiled a new mentorship program aimed at providing support and guidance to recent graduates as they navigate the transition from academia to the professional world. The program pairs recent graduates with experienced alumni mentors based on their career interests and goals, ensuring tailored guidance for each mentee. In addition to one-on-one mentorship, the program offers workshops on resume building, interview preparation, and networking strategies to equip graduates with essential skills for success. The mentorship program also facilitates networking events where mentees can expand their professional connections and learn from alumni who have excelled in their respective fields. By fostering a supportive network of alumni mentors, the program aims to empower recent graduates to navigate the challenges of the job market, enhance their professional growth, and build lasting connections within their industries. The launch of this mentorship program by the Alumni Association of University Y underscores the commitment to nurturing the success of its graduates beyond graduation, creating a strong community of support and guidance for the next generation of professionals.', N'Internet', 4, 1, 3, 3, '2024-05-05 00:00:00', 'https://images.unsplash.com/photo-1521737604893-d14cc237f11d?w=800', 340);
INSERT INTO dbo.NewsArticle (NewsArticleID, NewsTitle, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID, UpdatedByID, ModifiedDate, ImageUrl, ViewCount) VALUES (N'3', N'Academic Department Announces Groundbreaking Initiatives and Program Enhancements', N'Academic Department Announces Groundbreaking Initiatives and Program Enhancements', '2024-05-05 00:00:00', N'The Software Engineering Department at FU has unveiled a series of transformative initiatives and program enhancements aimed at enriching the academic experience and fostering innovation in Software Development. The department has established specific research centers or facilities dedicated to advancing knowledge and addressing pressing challenges in Software Development. Faculty Promotions: Several esteemed faculty members have been promoted to key leadership positions, reflecting their commitment to academic excellence and their vision for shaping the future of Software Development. The academic programs within the department have undergone enhancements to incorporate the latest developments and equip students with practical skills and knowledge relevant to current industry demands. These initiatives are poised to position the Software Engineering Department as a hub of innovation and academic rigor, attracting top talent and fostering groundbreaking research and learning experiences.', N'N/A', 1, 1, 4, 4, '2024-05-05 00:00:00', 'https://images.unsplash.com/photo-1531482615713-2afd69097998?w=800', 89);
INSERT INTO dbo.NewsArticle (NewsArticleID, NewsTitle, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID, UpdatedByID, ModifiedDate, ImageUrl, ViewCount) VALUES (N'4', N'Renowned Scholar Appointed as Head of AI Department at FU', N'Renowned Scholar Appointed as Head of AI Department at FU', '2024-05-05 00:00:00', N'FU proudly announces the appointment of David Nitzevet, a distinguished scholar in Machine Learning, to the prestigious position of Head of AI Department, underscoring the institution''s commitment to academic excellence and leadership. David Nitzevet brings a wealth of experience and expertise to the role, with a notable track record of the development of deep learning algorithms and the application of machine learning in healthcare, finance, and marketing. In accepting the appointment, David Nitzevet expressed a vision to develop new algorithmic models, improve data preprocessing techniques, and enhance the interpretability of machine learning models, align with the university''s mission to drive innovation and excellence in Machine Learning. The appointment is expected to foster collaborations and initiatives that will enrich the academic and research landscape of the university and beyond. The addition of David Nitzevet to the AI Department faculty elevates the institution''s academic standing and promises to inspire students, scholars, and professionals in Machine Learning. The appointment reaffirms the university''s dedication to recruiting top-tier talent and nurturing an environment where academic distinction thrives.', N'N/A', 1, 1, 4, 4, '2024-05-05 00:00:00', 'https://images.unsplash.com/photo-1507679799987-c73779587ccf?w=800', 210);
INSERT INTO dbo.NewsArticle (NewsArticleID, NewsTitle, Headline, CreatedDate, NewsContent, NewsSource, CategoryID, NewsStatus, CreatedByID, UpdatedByID, ModifiedDate, ImageUrl, ViewCount) VALUES (N'5', N'New Research Findings Shed Light on STEM', N'New Research Findings Shed Light on STEM', '2024-05-05 00:00:00', N'Groundbreaking research conducted by the Research Department of FU has unveiled significant findings in the field of STEM, offering fresh insights that could revolutionize current understanding and practices. The success of this research is attributed to the collaborative efforts of a multidisciplinary team, showcasing the institution''s commitment to fostering cutting-edge research. The newly uncovered knowledge has the potential to influence Math, Engineering, Technology and shape future research endeavors, positioning the Research Department of FU as a leader in the advancement of STEM. The research findings stand as a testament to the institution''s dedication to impactful research and its contribution to the global knowledge base in STEM.', N'N/A', 1, 0, 5, 5, '2024-05-05 00:00:00', 'https://images.unsplash.com/photo-1532094349884-543bc11b234d?w=800', 45);
GO

-- =============================================
-- SEED DATA: NewsTag
-- =============================================
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'1', 5);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'1', 7);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'1', 9);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'2', 5);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'2', 7);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'2', 8);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'2', 9);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'3', 1);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'3', 8);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'3', 9);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'4', 1);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'4', 4);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'4', 8);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'4', 9);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'5', 2);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'5', 3);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'5', 4);
INSERT INTO dbo.NewsTag (NewsArticleID, TagID) VALUES (N'5', 6);
GO
