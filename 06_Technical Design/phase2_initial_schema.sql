-- Phase 2 initial relational design
-- This is a design starting point. Review it with the team before migration creation.

CREATE TABLE TrainingModules (
    ModuleId INT IDENTITY PRIMARY KEY,
    Title NVARCHAR(200) NOT NULL,
    Purpose NVARCHAR(MAX) NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME()
);

CREATE TABLE QuestionBanks (
    QuestionBankId INT IDENTITY PRIMARY KEY,
    ModuleId INT NOT NULL,
    BankCode CHAR(1) NOT NULL,
    Title NVARCHAR(200) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_QuestionBanks_TrainingModules FOREIGN KEY (ModuleId) REFERENCES TrainingModules(ModuleId),
    CONSTRAINT UQ_QuestionBanks_Module_BankCode UNIQUE (ModuleId, BankCode),
    CONSTRAINT CK_QuestionBanks_BankCode CHECK (BankCode IN ('A','B','C'))
);

CREATE TABLE Questions (
    QuestionId INT IDENTITY PRIMARY KEY,
    QuestionBankId INT NOT NULL,
    QuestionText NVARCHAR(MAX) NOT NULL,
    Explanation NVARCHAR(MAX) NOT NULL,
    BusinessImpact NVARCHAR(MAX) NOT NULL,
    ReferenceText NVARCHAR(MAX) NOT NULL,
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT FK_Questions_QuestionBanks FOREIGN KEY (QuestionBankId) REFERENCES QuestionBanks(QuestionBankId),
    CONSTRAINT UQ_Questions_Bank_Order UNIQUE (QuestionBankId, DisplayOrder)
);

CREATE TABLE AnswerOptions (
    AnswerOptionId INT IDENTITY PRIMARY KEY,
    QuestionId INT NOT NULL,
    OptionCode CHAR(1) NOT NULL,
    OptionText NVARCHAR(MAX) NOT NULL,
    IsCorrect BIT NOT NULL DEFAULT 0,
    CONSTRAINT FK_AnswerOptions_Questions FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId),
    CONSTRAINT UQ_AnswerOptions_Question_Code UNIQUE (QuestionId, OptionCode)
);

-- ApplicationUser and roles should normally be created by ASP.NET Core Identity.
CREATE TABLE AssessmentAttempts (
    AttemptId BIGINT IDENTITY PRIMARY KEY,
    UserId NVARCHAR(450) NOT NULL,
    ModuleId INT NOT NULL,
    QuestionBankId INT NOT NULL,
    StartedUtc DATETIME2 NOT NULL DEFAULT SYSUTCDATETIME(),
    CompletedUtc DATETIME2 NULL,
    Status NVARCHAR(30) NOT NULL DEFAULT 'Started',
    Score DECIMAL(5,2) NULL,
    CONSTRAINT FK_AssessmentAttempts_TrainingModules FOREIGN KEY (ModuleId) REFERENCES TrainingModules(ModuleId),
    CONSTRAINT FK_AssessmentAttempts_QuestionBanks FOREIGN KEY (QuestionBankId) REFERENCES QuestionBanks(QuestionBankId)
);

CREATE TABLE UserAnswers (
    UserAnswerId BIGINT IDENTITY PRIMARY KEY,
    AttemptId BIGINT NOT NULL,
    QuestionId INT NOT NULL,
    SelectedOptionCode CHAR(1) NULL,
    IsCorrect BIT NULL,
    AnsweredUtc DATETIME2 NULL,
    CONSTRAINT FK_UserAnswers_Attempts FOREIGN KEY (AttemptId) REFERENCES AssessmentAttempts(AttemptId),
    CONSTRAINT FK_UserAnswers_Questions FOREIGN KEY (QuestionId) REFERENCES Questions(QuestionId),
    CONSTRAINT UQ_UserAnswers_Attempt_Question UNIQUE (AttemptId, QuestionId)
);

CREATE INDEX IX_AssessmentAttempts_User_Module ON AssessmentAttempts(UserId, ModuleId);
CREATE INDEX IX_Questions_Bank ON Questions(QuestionBankId);
