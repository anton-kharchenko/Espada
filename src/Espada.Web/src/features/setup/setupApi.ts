import { bffGet, bffPost } from 'shared/api';
import {
  setupCommitSchema,
  setupPreviewSchema,
  type SetupCommit,
  type SetupCommitRequest,
  type SetupPreview,
} from './setup';

export const getSetupPreview = (path: string, signal?: AbortSignal): Promise<SetupPreview> =>
  bffGet(`/bff/setup/preview?path=${encodeURIComponent(path)}`, setupPreviewSchema.parse, signal);

export const commitSetup = (request: SetupCommitRequest): Promise<SetupCommit> =>
  bffPost('/bff/setup/commit', request, setupCommitSchema.parse);
