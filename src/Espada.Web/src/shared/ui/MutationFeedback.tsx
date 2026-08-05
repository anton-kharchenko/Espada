interface MutationFeedbackProps {
  error: unknown;
  successMessage?: string;
  isSuccess?: boolean;
}

export const MutationFeedback = ({ error, successMessage, isSuccess = false }: MutationFeedbackProps) => {
  if (error instanceof Error) {
    return (
      <p className="form-feedback form-feedback-error" role="alert">
        {error.message}
      </p>
    );
  }

  if (isSuccess && successMessage) {
    return (
      <p className="form-feedback form-feedback-success" role="status">
        {successMessage}
      </p>
    );
  }

  return null;
};
