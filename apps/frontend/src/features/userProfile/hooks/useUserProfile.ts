import { useQuery } from "@tanstack/react-query";
import { useAuth0 } from "@auth0/auth0-react";
import { useApiFetcher } from "../../auth/hooks/useApiFetcher";
import { createUserProfileApi } from "../services/userProfileApi";

export const USER_PROFILE_QUERY_KEY = ["userProfile"] as const;

export function useUserProfile() {
  const { isAuthenticated } = useAuth0();
  const fetcher = useApiFetcher();
  const api = createUserProfileApi(fetcher);

  return useQuery({
    queryKey: USER_PROFILE_QUERY_KEY,
    queryFn: api.getProfile,
    enabled: isAuthenticated,
  });
}
