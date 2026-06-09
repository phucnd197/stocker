import { z } from 'zod';

export const authUserSchema = z.object({
  sub: z.string(),
  email: z.string().email().optional(),
  name: z.string().optional(),
  nickname: z.string().optional(),
  picture: z.string().url().optional(),
});

export type AuthUser = z.infer<typeof authUserSchema>;
