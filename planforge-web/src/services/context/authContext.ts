import apiClient, { setAccessToken } from "./";

export interface LoginRequest {
  email: string;
  password: string;
}

export interface LoginResponse {
  token: string;
  user: string;
  memberships: Membership[];
}

export interface Membership {
  organizationId: string;
  role: Roles;
}

const Roles = {
  Owner: 0,
  Admin: 1,
  Member: 2,
  Viewer: 4,
};

export type Roles = (typeof Roles)[keyof typeof Roles];

export const authService = {
  login: async (credentials: LoginRequest) => {
    const { LoginResponse: data } = await apiClient.post<Credential>(
      "/Auth/login",
      credentials,
    );

    setAccessToken(data.token);
    return data.user;
  },

  logout: async () => {
    await apiClient.post("/auth/logout");
    setAccessToken(null);
  },

  register: async (registerData: LoginRequest) => {
    const { LoginResponse: response } = await apiClient.post<LoginRequest>(
      "/Auth/register",
      registerData,
    );

    setAccessToken(response.token);
    return response.user;
  },
};
